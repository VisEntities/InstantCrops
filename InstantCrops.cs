using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Instant Crops", "VisEntities", "1.1.0")]
    [Description("Fully mature plants as soon as they are planted.")]
    public class InstantCrops : RustPlugin
    {
        #region Fields

        private static InstantCrops _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Seeds")]
            public List<SeedConfig> Seeds { get; set; }
        }

        private class SeedConfig
        {
            [JsonProperty("Item Short Name")]
            public string ItemShortName { get; set; }

            [JsonProperty("Immediate Ripeness In Planters")]
            public bool ImmediateRipenessInPlanter { get; set; }

            [JsonProperty("Immediate Ripeness In Ground")]
            public bool ImmediateRipenessInGround { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                Seeds = new List<SeedConfig>
                {
                    new SeedConfig
                    {
                        ItemShortName = "seed.pumpkin",
                        ImmediateRipenessInPlanter = false,
                        ImmediateRipenessInGround = true
                    },
                    new SeedConfig
                    {
                        ItemShortName = "seed.hemp",
                        ImmediateRipenessInPlanter = false,
                        ImmediateRipenessInGround = true
                    },
                    new SeedConfig
                    {
                        ItemShortName = "seed.potato",
                        ImmediateRipenessInPlanter = false,
                        ImmediateRipenessInGround = true
                    },
                    new SeedConfig
                    {
                        ItemShortName = "seed.corn",
                        ImmediateRipenessInPlanter = false,
                        ImmediateRipenessInGround = true
                    },
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnEntityBuilt(Planner planner, GameObject gameObject)
        {
            if (planner == null || gameObject == null)
                return;

            BasePlayer player = planner.GetOwnerPlayer();
            if (player == null)
                return;

            if (!PermissionUtil.HasPermission(player, PermissionUtil.USE))
                return;

            Item activeItem = player.GetActiveItem();
            if (activeItem == null)
                return;

            BaseEntity entity = gameObject.ToBaseEntity();
            if (entity == null)
                return;

            GrowableEntity growableEntity = entity as GrowableEntity;
            if (growableEntity == null)
                return;

            string seedShortName = activeItem.info.shortname;

            SeedConfig seedConfig = _config.Seeds.Find(s => s.ItemShortName == seedShortName);
            if (seedConfig == null)
                return;

            NextTick(() =>
            {
                PlanterBox planterBox = growableEntity.GetPlanter();
                if (planterBox != null && seedConfig.ImmediateRipenessInPlanter)
                {
                    growableEntity.ChangeState(PlantProperties.State.Ripe, false);
                }
                else if (planterBox == null && seedConfig.ImmediateRipenessInGround)
                {
                    growableEntity.ChangeState(PlantProperties.State.Ripe, false);
                }
            });
        }

        #endregion Oxide Hooks

        #region Permissions

        private static class PermissionUtil
        {
            public const string USE = "instantcrops.use";
            private static readonly List<string> _permissions = new List<string>
            {
                USE,
            };

            public static void RegisterPermissions()
            {
                foreach (var permission in _permissions)
                {
                    _plugin.permission.RegisterPermission(permission, _plugin);
                }
            }

            public static bool HasPermission(BasePlayer player, string permissionName)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Permission
    }
}