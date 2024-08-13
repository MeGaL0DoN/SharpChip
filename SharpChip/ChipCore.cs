using System.Runtime.CompilerServices;

namespace SharpChip
{
    public class ChipCore
    {
        public bool RomLoaded { get; private set; }
        public void LoadRom(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                if (fs.Length < (RAM_SIZE - 0x200))
                {
                    reset();
                    fs.Read(RAM, 0x200, (int)(fs.Length));
                    RomLoaded = true;
                }
            }
        }
        private void reset()
        {
            fontset.CopyTo(RAM, 0);
            PC = 0x200;
            SP = 0;
            I = 0;
            delayTimer = 0;
            soundTimer = 0;
            keyWaitReg = -1;

            Array.Clear(RAM, 0, RAM.Length);
            Array.Clear(V, 0, V.Length);
            Array.Clear(screenBuffer, 0, screenBuffer.Length);
        }

        public void drawToBuffer(Bitmap buf)
        {
            for (int y = 0; y < SCR_HEIGHT; y++)
            {
                long rowData = screenBuffer[y];

                for (int x = 0; x < SCR_WIDTH; x++)
                {
                    bool pixelOn = ((rowData >> (63 - x)) & 0x1) != 0;
                    buf.SetPixel(x, y, pixelOn ? Color.White : Color.Black);
                }
            }
        }

        public void setKey(int key, bool val)
        {
            keys[key & 0xF] = val;

            if (keyWaitReg != -1 && !val)
            {
                V[keyWaitReg] = (byte)key;
                keyWaitReg = -1;
            }
        }

        public void updateTimers()
        {
            if (delayTimer > 0) delayTimer--;
            if (soundTimer > 0) soundTimer--;
        }

        public void execute()
        {
            if (keyWaitReg != -1) return;

            opcode = (ushort)(RAM[PC] << 8 | RAM[PC + 1]);
            PC += 2;

            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000:
                            Array.Clear(screenBuffer, 0, screenBuffer.Length);
                            break;
                        case 0x000E:
                            PC = stack[--SP & 0xF];
                            break;
                    }
                    break;
                case 0x1000:
                    PC = Addr();
                    break;
                case 0x2000:
                    stack[SP++ & 0xF] = PC;
                    PC = Addr();
                    break;
                case 0x3000:
                    if (V[X()] == Data()) PC += 2;
                    break;
                case 0x4000:
                    if (V[X()] != Data()) PC += 2;
                    break;
                case 0x5000:
                    if (V[X()] == V[Y()]) PC += 2;
                    break;
                case 0x6000:
                    V[X()] = Data();
                    break;
                case 0x7000:
                    V[X()] += Data();
                    break;
                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000:
                            V[X()] = V[Y()];
                            break;
                        case 0x0001:
                            V[X()] |= V[Y()];
                            V[0xF] = 0;
                            break;
                        case 0x0002:
                            V[X()] &= V[Y()];
                            V[0xF] = 0;
                            break;
                        case 0x0003:
                            V[X()] ^= V[Y()];
                            V[0xF] = 0;
                            break;
                        case 0x0004:
                            V[X()] += V[Y()];
                            V[0xF] = (V[X()] < V[Y()]) ? (byte)1 : (byte)0;
                            break;
                        case 0x0005:
                            byte underflow = (V[Y()] > V[X()]) ? (byte)0 : (byte)1;
                            V[X()] -= V[Y()];
                            V[0xF] = underflow;
                            break;
                        case 0x0006:
                            byte shifted = (byte)(V[X()] & 0x1);
                            V[X()] >>= 1;
                            V[0xF] = shifted;
                            break;
                        case 0x0007:
                            V[X()] = (byte)(V[Y()] - V[X()]);
                            V[0xF] = (V[Y()] >= V[X()]) ? (byte)1 : (byte)0;
                            break;
                        case 0x000E:
                            shifted = (byte)((V[X()] & 0b10000000) >> 7);
                            V[X()] <<= 1;
                            V[0xF] = shifted;
                            break;
                    }
                    break;
                case 0x9000:
                    if (V[X()] != V[Y()]) PC += 2;
                    break;
                case 0xA000:
                    I = Addr();
                    break;
                case 0xB000:
                    PC = (ushort)(Addr() + V[0]);
                    break;
                case 0xC000:
                    V[X()] = (byte)(Random.Shared.Next() & Data());
                    break;
                case 0xD000:
                    DXYN((byte)(V[X()] & (SCR_WIDTH - 1)), (byte)(V[Y()] & (SCR_HEIGHT - 1)), (byte)(opcode & 0xF));
                    break;
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E:
                            if (keys[V[X()] & 0xF]) PC += 2;
                            break;
                        case 0x00A1:
                            if (!keys[V[X()] & 0xF]) PC += 2;
                            break;
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007:
                            V[X()] = delayTimer;
                            break;
                        case 0x000A:
                            keyWaitReg = (sbyte)X();
                            break;
                        case 0x0015:
                            delayTimer = V[X()];
                            break;
                        case 0x0018:
                            soundTimer = V[X()];
                            break;
                        case 0x001E:
                            I += V[X()];
                            break;
                        case 0x0029:
                            I = (ushort)((V[X()] & 0xF) * 0x5);
                            break;
                        case 0x0033:
                            byte vx = V[X()];
                            RAM[I] = (byte)(vx / 100);
                            RAM[I + 1] = (byte)((vx / 10) % 10);
                            RAM[I + 2] = (byte)(vx % 10);
                            break;
                        case 0x0055:
                            for (int i = 0; i <= X(); i++)
                                RAM[(I + i) & 0xFFF] = V[i];
                            break;
                        case 0x0065:
                            for (int i = 0; i <= X(); i++)
                                V[i] = RAM[(I + i) & 0xFFF];
                            break;
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte X()
        {
            return (byte)((opcode & 0x0F00) >> 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte Y()
        {
            return (byte)((opcode & 0x00F0) >> 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort Addr()
        {
            return (ushort)(opcode & 0x0FFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte Data()
        {
            return (byte)(opcode & 0xFF);
        }

        const short RAM_SIZE = 4096;
        byte[] RAM = new byte[RAM_SIZE];

        ushort PC;
        byte[] V = new byte[16];
        ushort[] stack = new ushort[16];
        short SP;
        byte delayTimer;
        byte soundTimer;
        ushort I;
        ushort opcode;

        bool[] keys = new bool[16];
        sbyte keyWaitReg;

        const byte SCR_WIDTH = 64;
        const byte SCR_HEIGHT = 32;
        long[] screenBuffer = new long[SCR_HEIGHT];

        private byte[] fontset = new byte[]
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0,		// 0
			0x20, 0x60, 0x20, 0x20, 0x70,		// 1
			0xF0, 0x10, 0xF0, 0x80, 0xF0,		// 2
			0xF0, 0x10, 0xF0, 0x10, 0xF0,		// 3
			0x90, 0x90, 0xF0, 0x10, 0x10,		// 4
			0xF0, 0x80, 0xF0, 0x10, 0xF0,		// 5
			0xF0, 0x80, 0xF0, 0x90, 0xF0,		// 6
			0xF0, 0x10, 0x20, 0x40, 0x40,		// 7
			0xF0, 0x90, 0xF0, 0x90, 0xF0,		// 8
			0xF0, 0x90, 0xF0, 0x10, 0xF0,		// 9
			0xF0, 0x90, 0xF0, 0x90, 0x90,		// A
			0xE0, 0x90, 0xE0, 0x90, 0xE0,		// B
			0xF0, 0x80, 0x80, 0x80, 0xF0,		// C
			0xE0, 0x90, 0x90, 0x90, 0xE0,		// D
			0xF0, 0x80, 0xF0, 0x80, 0xF0,		// E
			0xF0, 0x80, 0xF0, 0x80, 0x80		// F
		};

        void DXYN(byte Xpos, byte Ypos, byte height)
        {
            V[0xF] = 0;
            bool partialDraw = Xpos > 56;

            for (int i = 0; i < height; i++)
            {
                byte spriteRow = RAM[(I + i) & 0xFFF];

                if (Ypos >= SCR_HEIGHT)
                    break;

                long spriteMask;

                if (partialDraw)
                {
                    long leftPart = ((long)spriteRow >> (Xpos - 56));
                    spriteMask = leftPart;
                }
                else
                    spriteMask = ((long)spriteRow << (63 - Xpos - 7));

                long screenRow = screenBuffer[Ypos];
                V[0xF] |= (byte)((screenRow & spriteMask) != 0 ? 1 : 0);

                screenBuffer[Ypos] ^= spriteMask;
                Ypos++;
            }
        }
    }
}
