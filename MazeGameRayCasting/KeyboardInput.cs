using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MazeGameRayCasting
{
    public static class KeyboardInput
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Virtual key codes
        public const int VK_W = 0x57;
        public const int VK_A = 0x41;
        public const int VK_S = 0x53;
        public const int VK_D = 0x44;
        public const int VK_ESCAPE = 0x1B;

        // Check if a key is currently pressed
        public static bool IsKeyPressed(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }
    }
}
