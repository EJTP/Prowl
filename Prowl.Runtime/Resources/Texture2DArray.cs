using Veldrid;
using System;

namespace Prowl.Runtime
{
    /// <summary>
    /// A <see cref="Texture"/> comprised of an array of images with two dimensions and support for multisampling.
    /// </summary>
    public sealed class Texture2DArray : Texture, ISerializable
    {
        /// <summary>The width of this <see cref="Texture2DArray"/>.</summary>
        public uint Width => InternalTexture.Width;

        /// <summary>The height of this <see cref="Texture2DArray"/>.</summary>
        public uint Height => InternalTexture.Height;
        
        /// <summary>The quantity or length of this <see cref="Texture2DArray"/>.</summary>
        public uint Layers => InternalTexture.ArrayLayers;

        /// <summary>
        /// Creates a <see cref="Texture2DArray"/> with the desired parameters but no image data.
        /// </summary>
        /// <param name="width">The width of the <see cref="Texture2DArray"/>.</param>
        /// <param name="height">The height of the <see cref="Texture2DArray"/>.</param>
        /// <param name="layers">The height of the <see cref="Texture2DArray"/>.</param>
        /// <param name="mipLevels">How many mip levels this texcture has <see cref="Texture3D"/>.</param>
        /// <param name="format">The pixel format for this <see cref="Texture3D"/>.</param>
        public Texture2DArray(uint width, uint height, uint layers, uint mipLevels = 0, PixelFormat format = PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage usage = TextureUsage.Sampled) : base(new() 
            {
                Width = width,
                Height = height,
                Depth = 1,
                MipLevels = mipLevels,
                ArrayLayers = layers,
                Format = format,
                Usage = usage,
                Type = TextureType.Texture2D,
            }) { }

        
        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <param name="ptr">The pointer from which the pixel data will be read.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="layerIndex">The layer index of the array to write to.</param>
        /// <param name="mipLevel">The mip level to write to.</param>
        public unsafe void SetDataPtr(void* ptr, uint rectX, uint rectY, uint rectWidth, uint rectHeight, uint layerIndex, uint mipLevel = 0) =>
            InternalSetDataPtr(ptr, new Vector3Int((int)rectX, (int)rectY, 0), new Vector3Int((int)rectWidth, (int)rectHeight, 0), layerIndex, mipLevel);

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Memory{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="layerIndex">The layer index of the array to write to.</param>
        /// <param name="mipLevel">The mip level to write to.</param>
        public unsafe void SetData<T>(Memory<T> data, uint rectX, uint rectY, uint rectWidth, uint rectHeight, uint layerIndex, uint mipLevel = 0) where T : unmanaged =>
            InternalSetData(data, new Vector3Int((int)rectX, (int)rectY, 0), new Vector3Int((int)rectWidth, (int)rectHeight, 0), layerIndex, mipLevel);

        /// <summary>
        /// Sets the data of an entire layer of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        public void SetData<T>(Memory<T> data, uint layerIndex, uint mipLevel = 0) where T : unmanaged =>
            SetData(data, 0, 0, Width, Height, layerIndex, mipLevel);

        /// <summary>
        /// Copies the data of a portion of a <see cref="Texture2DArray"/>.
        /// </summary>
        /// <param name="data">The pointer to the copied .</param>
        /// <param name="mipLevel">The mip level to copy.</param>
        /// <param name="layer">The array layer to copy.</param>
        public unsafe void CopyDataPtr(void* data, uint layer, uint mipLevel = 0) =>
            InternalCopyDataPtr(data, out _, out _, (MipLevels * layer) + mipLevel);

        /// <summary>
        /// Copies the data of a portion of a <see cref="Texture2DArray"/> into a CPU-accessible region.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> in which to write the pixel data.</param>
        /// <param name="mipLevel">The mip level to copy.</param>
        /// <param name="layer">The array layer to copy.</param>
        public unsafe void CopyData<T>(Memory<T> data, uint layer, uint mipLevel = 0) where T : unmanaged =>
            InternalCopyData(data, (MipLevels * layer) + mipLevel);

        /// <summary>
        /// Recreates and resizes the <see cref="Texture2DArray"/>.
        /// </summary>
        public void RecreateTexture(uint width, uint height, uint layers)
        {
            RecreateInternalTexture(new()
            {
                Width = width,
                Height = height,
                Depth = 1,
                MipLevels = this.MipLevels,
                ArrayLayers = layers,
                Format = this.Format,
                Usage = this.Usage,
                Type = this.Type,
            });
        }

        public uint GetSingleTextureMemoryUsage()
        {
            return Width * Height * PixelFormatBytes(Format);
        }

        public SerializedProperty Serialize(Serializer.SerializationContext ctx)
        {
            SerializedProperty compoundTag = SerializedProperty.NewCompound();
            compoundTag.Add("Width", new((int)Width));
            compoundTag.Add("Height", new((int)Height));
            compoundTag.Add("Layers", new((int)Layers));
            compoundTag.Add("MipLevels", new(MipLevels));
            compoundTag.Add("IsMipMapped", new(IsMipmapped));
            compoundTag.Add("ImageFormat", new((int)Format));
            compoundTag.Add("Usage", new((int)Usage));

            for (uint i = 0; i < Layers; i++)
            {
                Memory<byte> memory = new byte[GetSingleTextureMemoryUsage()];
                CopyData(memory, i);
                compoundTag.Add($"Data{i}", new(memory.ToArray()));
            }

            return compoundTag;
        }

        public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
        {
            uint width = (uint)value["Width"].IntValue;
            uint height = (uint)value["Height"].IntValue;
            uint layers = (uint)value["Layers"].IntValue;
            uint mips = (uint)value["MipLevels"].IntValue;
            bool isMipMapped = value["IsMipMapped"].BoolValue;
            PixelFormat imageFormat = (PixelFormat)value["ImageFormat"].IntValue;
            TextureUsage usage = (TextureUsage)value["Usage"].IntValue;

            var param = new[] { typeof(uint), typeof(uint), typeof(uint), typeof(uint), typeof(PixelFormat), typeof(TextureUsage)  };
            var values = new object[] { width, height, layers, mips, imageFormat, usage };

            typeof(Texture2DArray).GetConstructor(param).Invoke(this, values);
                
            for (uint i = 0; i < layers; i++)
            {
                Memory<byte> memory = value[$"Data{i}"].ByteArrayValue;
                SetData(memory, i);
            }

            if (isMipMapped)
                GenerateMipmaps();
        }
    }
}
