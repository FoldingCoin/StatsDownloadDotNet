﻿namespace StatsDownload.FileServer.TestHarness.CastleWindsor
{
    using Castle.Facilities.WcfIntegration;
    using Castle.Windsor;

    using CastleWindsorContainer = Castle.Windsor.WindsorContainer;

    internal static class WindsorContainer
    {
        private static IWindsorContainer innerContainer;

        private static readonly object sync = new object();

        internal static IWindsorContainer Instance
        {
            get
            {
                lock (sync)
                {
                    return innerContainer ?? (innerContainer = CreateWindsorContainer());
                }
            }
        }

        public static void Dispose()
        {
            lock (sync)
            {
                innerContainer?.Dispose();
                innerContainer = null;
            }
        }

        private static IWindsorContainer CreateWindsorContainer()
        {
            var container = new CastleWindsorContainer();
            container.AddFacility<WcfFacility>();
            return container;
        }
    }
}