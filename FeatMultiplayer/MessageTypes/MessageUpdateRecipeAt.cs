// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateRecipeAt : MessageBase
    {
        const string messageCode = "UpdateRecipeAt";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal string codeName;

        public void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            var content = Plugin.ContentAt(coords);
            if (content is CItem_ContentFactory factory)
            {
                int recipeIndex = factory.dataRecipe.GetValue(coords);
                codeName = factory.recipes[recipeIndex].outputs[0].item.codeName;
            }
        }

        public void CreateRequest(int2 coords, string codeName)
        {
            this.coords = coords;
            this.codeName = codeName;
        }

        public void ApplySnapshot()
        {
            var lookup = Plugin.GetItemsDictionary();
            var content = Plugin.ContentAt(coords);
            if (content is CItem_ContentFactory factory)
            {
                int recipeIndex = Array.FindIndex(factory.recipes, x => x.outputs[0].item.codeName == codeName);
                if (recipeIndex >= 0)
                {
                    factory.ChangeRecipeIFN(coords, recipeIndex);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(codeName);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateRecipeAt();
            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            codeName = input.ReadString();
        }
    }
}
