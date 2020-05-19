using System;
using System.Collections.Generic;
using SharpDX.DXGI;

namespace Veldrid.SceneGraph.Wpf.Element
{
    public static class DeviceUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static int AdapterCount
        {
            get
            {
                if (sAdapterCount == -1)
                    using (var f = new Factory(IntPtr.Zero))
                        sAdapterCount = f.GetAdapterCount();
                return sAdapterCount;
            }
        }

        /// <summary>
        ///  cache it, as the underlying code rely on Exception to find the value!!!
        /// </summary>
        private static int sAdapterCount = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        public static IEnumerable<Adapter> GetAdapters(DisposeGroup dg)
        {
            // NOTE: SharpDX 1.3 requires explicit Dispose() of everything
            // hence the DisposeGroup, to enforce it
            using (var f = new Factory(IntPtr.Zero))
            {
                int n = AdapterCount;
                for (int i = 0; i < n; i++)
                    yield return dg.Add(f.GetAdapter(i));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        public static Adapter GetBestAdapter(DisposeGroup dg)
        {
            SharpDX.Direct3D.FeatureLevel high = SharpDX.Direct3D.FeatureLevel.Level_9_1;
            Adapter ada = null;
            foreach (var item in GetAdapters(dg))
            {
                var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(item);
                if (ada == null || level > high)
                {
                    ada = item;
                    high = level;
                }
            }
            return ada;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cFlags"></param>
        /// <param name="minLevel"></param>
        /// <returns></returns>
        public static SharpDX.Direct3D11.Device Create11(
            SharpDX.Direct3D11.DeviceCreationFlags cFlags = SharpDX.Direct3D11.DeviceCreationFlags.None,
            SharpDX.Direct3D.FeatureLevel minLevel = SharpDX.Direct3D.FeatureLevel.Level_9_1)
        {
            using (var dg = new DisposeGroup())
            {
                var ada = GetBestAdapter(dg);
                if (ada == null)
                    return null;
                var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(ada);
                if (level < minLevel)
                    return null;
                return new SharpDX.Direct3D11.Device(ada, cFlags, level);
            }
        }
    }
}
