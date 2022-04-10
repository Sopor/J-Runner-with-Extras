﻿using System;
using System.Windows;

namespace JRunner.Nand
{
    public class PatchParser
    {
        public UInt32 address;
        public UInt32 patchCount;
        public UInt32[] patches;
        public int index;
        public byte[] patchArray;

        public PatchParser(byte[] data)
        {
            patchArray = data;
        }

        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public UInt32 getAddress(byte[] data)
        {
            address = ReverseBytes(BitConverter.ToUInt32(patchArray, index));
            index += 4;
            return address;
        }

        public UInt32 getCount(byte[] data)
        {
            patchCount = ReverseBytes(BitConverter.ToUInt32(patchArray, index));
            index += 4;
            return patchCount;
        }

        public UInt32[] getPatches(byte[] data)
        {
            patches = new UInt32[patchCount];
            for (int i = 0; i < patchCount; i++)
            {
                patches[i] = ReverseBytes(BitConverter.ToUInt32(patchArray, index));
                index += 4;
            }
            return patches;
        }

        public void printPatches()
        {
            foreach (var pat in patches)
            {
                Console.WriteLine("Patch: 0x" + pat.ToString("X8"));
            }
        }

        public void printAddress()
        {
            Console.WriteLine("Address: 0x" + address.ToString("X8"));
        }

        public void printPatchCount()
        {
            Console.WriteLine("Patch Count: 0x" + patchCount.ToString("X8"));
        }

        public bool parseAll(int i = 0)
        {
            bool foundAPatch = false;
            index = i;
            while (getAddress(patchArray) != (UInt32)0xFFFFFFFFU) //moves index+4
            {
                index -= 0x4; //return index to original position
                UInt32 detectd2m_devgl = getAddress(patchArray);

                if (detectd2m_devgl == 0x00000000 || detectd2m_devgl == 0xF0000000) //moves index to check
                {
                    index += 0x50; //devgl/g2m detect
                    index -= 0x4; //go back to original location + 0x50
                    continue; //iterate
                }
                else
                {
                    index -= 0x4; //return to original position
                }

                if (getAddress(patchArray) == 0x000E3A7C) //moves index and gets address
                {
                    if (getCount(patchArray) == 0x00000001) //moves index and gets count
                    {
                        UInt32[] patchlist;
                        patchlist = getPatches(patchArray);

                        if (patchlist[0] == 0x3CE02000)
                        {
                            foundAPatch = true;
                            Console.WriteLine("XL USB Patches applied!");
                            if (!variables.noPatchWarnings) MessageBox.Show("This NAND has XL USB patches applied, which only allows FATXplorer formatted storage devices to work.\n\nDevices formatted on the Xbox 360 will NOT work!\n\nIf you don't want this, generate an image without the XL USB checked under \"Patches/Dashlaunch\"", "AYO?", MessageBoxButton.OK,MessageBoxImage.Exclamation);
                        }
                    }
                }
                else
                {
                    index -= 0x4; //return to origin
                }

                getAddress(patchArray);

                if (getCount(patchArray)> 0x1000)
                {
                    //assume image has no patches
                    index = 0;
                    break;
                }
                getPatches(patchArray);
            }

            return foundAPatch;
        }

        public int getIndex()
        {
            return index;
        }

        public void resetIndex()
        {
            index = 0;
        }
    }
}
