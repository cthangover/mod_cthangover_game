using System.Linq;
using Cthangover.Core.Mods;
using Cthangover.Core.Utils;
using Godot;

namespace Cthangover.Mods.CthangoverGame.Tests
{
    /// <summary>
    /// Entry point of the site_shots suite: adds the driver to the tree root when
    /// the exact flag <c>--itest=site_shots</c> is given (the driver quits the
    /// process, so it must never run in another test). <c>--shots=&lt;dir&gt;</c>
    /// is where the pictures go; without it the driver only walks the steps.
    /// </summary>
    public class SiteShotsTestInitializer : IModInitializer
    {
        private static bool driverQueued;

        /// <summary>Nothing per mod.</summary>
        public void OnModLoaded(string modId)
        {
        }

        /// <summary>Queues the driver once.</summary>
        public void OnModResourcesReady()
        {
            var args = OS.GetCmdlineUserArgs();
            if (driverQueued || !args.Any(a => a == "--itest=site_shots"))
                return;

            driverQueued = true;
            var shots = args.Where(a => a.StartsWith("--shots=", System.StringComparison.Ordinal))
                .Select(a => a.Substring("--shots=".Length))
                .FirstOrDefault();
            GameLogger.Log("TEST", $"SiteShotsTestInitializer: activating site_shots, shots={shots ?? "off"}");
            (Godot.Engine.GetMainLoop() as SceneTree)?.Root?.CallDeferred("add_child", new SiteShotsDriver(shots));
        }
    }
}
