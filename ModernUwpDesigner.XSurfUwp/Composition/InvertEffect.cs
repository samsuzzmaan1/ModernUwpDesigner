using System;
using System.Runtime.InteropServices;
using Windows.Graphics.Effects;
using WinRT;

namespace XSurfUwp.Composition
{
    [Guid("E0C3784D-CB39-4E84-B6FD-6B72F0810263")]
    internal sealed partial class InvertEffect : IGraphicsEffect, IGraphicsEffectSource, IGraphicsEffectD2D1Interop
    {
        private string _name = "InvertEffect";
        private Guid _id = new("E0C3784D-CB39-4E84-B6FD-6B72F0810263");

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public IGraphicsEffectSource Source { get; set; }

        public Guid GetEffectId() => _id;

        public GRAPHICS_EFFECT_PROPERTY_MAPPING GetNamedPropertyMapping(string name, out uint index) => throw new NotSupportedException();

        public object GetProperty(uint index) => throw new NotSupportedException();

        public uint GetPropertyCount() => 0;

        public IGraphicsEffectSource GetSource(uint index) => index switch
        {
            0 => Source,
            _ => null
        };

        public uint GetSourceCount() => 1;
    }
}
