using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    class Enigma
    {
        private const string reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT";
        private const string alphab = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string rotorA = "EKMFLGDQVZNTOWYHXUSPAIBRCJ";
        private const string rotorB = "AJDKSIRUXBLHWTMCQGZNPYFVOE";
        private const string rotorC = "BDFHJLCPRTXVZNYEIWGAKMUSQO";

        private Rotor[] rotors = new Rotor[3];
        private string plugBoard;

        private class Rotor
        {
            private string wiring;
            private int notch;
            private int position;
            private int ringSettings;
            private string wwiring;

            public Rotor(string wiring, int notch, int start, int ringSettings)
            {
                this.wwiring = wiring;
                this.notch = notch % 26;
                this.position = start % 26;
                this.ringSettings = ringSettings;
                this.wiring = wwiring.Substring(ringSettings, wwiring.Length - ringSettings) + wwiring.Substring(0, ringSettings);
            }

            public void Step()
            {
                position = (position + 1) % 26;
            }

            public char EncryptChar(char c, bool left)
            {
                char tmp = wiring[(char.ToUpper(c) - 65 + position) % 26];
                if (left)
                {
                    if ((char)(tmp - position) < 'A')
                    {
                        int a = tmp - position - 'A';
                        return (char)('Z' + a + 1);
                    }
                    return (char)(tmp - position);
                }
                int i = 0;

                while (wiring[i] != alphab[(c + position - 'A') % 26]) 
                    i++;
                i -= position;
                if (i < 0)
                    return (char)('Z' + i + 1);
                return (char)('A' + i);
            }

            public bool NotchRotate()
            {
                if (notch == position)
                    return true;
                return false;
            }
        }

        public Enigma(string rotors, string ringSettings, string plugboard, string rotorStart)
        {
            for (int i = 0; i < 3; i++)
            {
                switch (char.ToUpper(rotors[i]))
                {
                    case 'A':
                        this.rotors[i] = new Rotor(rotorA, 16, (char.ToUpper(rotorStart[i]) - 'A'), char.ToUpper(ringSettings[i]) - 'A');
                        break;
                    case 'B':
                        this.rotors[i] = new Rotor(rotorB, 4, (char.ToUpper(rotorStart[i]) - 'A'), char.ToUpper(ringSettings[i]) - 'A');
                        break;
                    case 'C':
                        this.rotors[i] = new Rotor(rotorC, 21, (char.ToUpper(rotorStart[i]) - 'A'), char.ToUpper(ringSettings[i]) - 'A');
                        break;
                    default:
                        break;
                }
            }
            this.plugBoard = plugboard;

        }

        private void Rotate()
        {
            if (rotors[2].NotchRotate())
            {
                rotors[1].Step();
            }
            else if (rotors[1].NotchRotate())
            {
                rotors[1].Step();
                rotors[0].Step();
            }
            rotors[2].Step();
        }

        public string EncryptString(string txt)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in txt)
            {
                Rotate();
                result.Append(EncryptChar(c));
            }
            return result.ToString();
        }

        private char EncryptChar(char c)
        {
            char result = plugBoard[char.ToUpper(c) - 'A'];
            result = rotors[2].EncryptChar(result, true);
            result = rotors[1].EncryptChar(result, true);
            result = rotors[0].EncryptChar(result, true);
            result = reflector[result - 'A'];
            result = rotors[0].EncryptChar(result, false);
            result = rotors[1].EncryptChar(result, false);
            result = rotors[2].EncryptChar(result, false);
            result = plugBoard[char.ToUpper(result) - 'A'];
            return result;
        }
    }
}
