using System.Collections.Generic;
using System.Diagnostics;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Web;
using WaveEngine.WebGL;
using WebAssembly;

namespace BetiJaiDemo.Web
{
    public class Program
    {
        private static readonly Dictionary<string, WebSurface> appCanvas = new Dictionary<string, WebSurface>();
        
        private static MyApplication application;

        public static void DisplayZone(int id) => application.DisplayZone(id);

        public static void Main(string canvasId)
        {
            // Create app
            application = new MyApplication();

            // Create Services
            var windowsSystem = new WebWindowsSystem();
            application.Container.RegisterInstance(windowsSystem);

            var document = (JSObject)Runtime.GetGlobalObject("document");
            var canvas = (JSObject)document.Invoke("getElementById", canvasId);
            var surface = (WebSurface)windowsSystem.CreateSurface(canvas);
            appCanvas[canvasId] = surface;
            ConfigureGraphicsContext(application, surface);

            // Audio is currently unsupported
            //var xaudio = new WaveEngine.XAudio2.XAudioDevice();
            //application.Container.RegisterInstance(xaudio);

            var notifier = new JavaScriptHotspotNotifier();
            application.Container.RegisterInstance<IHotspotNotifier>(notifier);

            Stopwatch clockTimer = Stopwatch.StartNew();
            windowsSystem.Run(
                () =>
                {
                    application.Initialize();
                    Runtime.InvokeJS("WaveEngine.init();");
                },
                () =>
                {
                    var gameTime = clockTimer.Elapsed;
                    clockTimer.Restart();

                    application.UpdateFrame(gameTime);
                    application.DrawFrame(gameTime);
                });
        }

        public void UpdateCanvasSize(string canvasId)
        {
            if (appCanvas.TryGetValue(canvasId, out var surface))
            {
                surface.RefreshSize();
            }
        }

        private static void ConfigureGraphicsContext(Application application, Surface surface)
        {
            GraphicsContext graphicsContext = new WebGLGraphicsContext();
            graphicsContext.CreateDevice();
            SwapChainDescription swapChainDescription = new SwapChainDescription()
            {
                SurfaceInfo = surface.SurfaceInfo,
                Width = surface.Width,
                Height = surface.Height,
                ColorTargetFormat = PixelFormat.R8G8B8A8_UNorm,
                ColorTargetFlags = TextureFlags.RenderTarget | TextureFlags.ShaderResource,
                DepthStencilTargetFormat = PixelFormat.D24_UNorm_S8_UInt,
                DepthStencilTargetFlags = TextureFlags.DepthStencil,
                SampleCount = TextureSampleCount.None,
                IsWindowed = true,
                RefreshRate = 60
            };
            var swapChain = graphicsContext.CreateSwapChain(swapChainDescription);
            swapChain.VerticalSync = true;

            var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
            var firstDisplay = new Display(surface, swapChain);
            graphicsPresenter.AddDisplay("DefaultDisplay", firstDisplay);

            application.Container.RegisterInstance(graphicsContext);
        }
    }
}
