﻿using System;

namespace Xunit
{
    public class GuidCounter :
        Counter<Guid>
    {
        protected override Guid Convert(int i)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(i).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}