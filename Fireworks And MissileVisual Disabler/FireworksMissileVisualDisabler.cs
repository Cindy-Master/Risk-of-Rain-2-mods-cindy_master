using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FireworksMissileVisualDisabler
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class FireworksMissileVisualDisabler : BaseUnityPlugin
    {
        public const string PluginGUID = "com.yourname.fireworksmissilevisualdisabler";
        public const string PluginName = "Fireworks Missile Visual Disabler";
        public const string PluginVersion = "1.0.0";

        // 配置选项
        private ConfigEntry<bool> disableFireworksVisuals;
        private ConfigEntry<bool> disableAtGMissileVisuals;
        private ConfigEntry<bool> disableFireworkActivationEffect;

        public void Awake()
        {
            // 创建配置选项
            disableFireworksVisuals = Config.Bind<bool>(
                "Visual Effects",
                "DisableFireworksVisuals",
                true,
                "禁用烟花束的视觉效果（保留伤害）"
            );

            disableAtGMissileVisuals = Config.Bind<bool>(
                "Visual Effects",
                "DisableAtGMissileVisuals",
                true,
                "禁用AtG导弹的视觉效果（保留伤害）"
            );

            disableFireworkActivationEffect = Config.Bind<bool>(
                "Visual Effects",
                "DisableFireworkActivationEffect",
                true,
                "禁用烟花道具触发时的庆祝粒子特效"
            );

            // 使用 Addressables 加载烟花视觉预制体
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkGhost.prefab").Completed += handle =>
            {
                if (handle.Result == null)
                {
                    Logger.LogError("Failed to load FireworkGhost.prefab");
                    return;
                }

                GameObject ghostPrefab = handle.Result;

                // 禁用拖尾渲染器
                TrailRenderer trail = ghostPrefab.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    disableFireworksVisuals.SettingChanged += (x, _) =>
                    {
                        trail.enabled = !disableFireworksVisuals.Value;
                    };
                    trail.enabled = !disableFireworksVisuals.Value;
                }

                // 禁用粒子系统
                ParticleSystem[] particles = ghostPrefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    disableFireworksVisuals.SettingChanged += (x, _) =>
                    {
                        if (disableFireworksVisuals.Value)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                    };
                    if (disableFireworksVisuals.Value)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                // 禁用所有渲染器（模型）
                Renderer[] renderers = ghostPrefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    disableFireworksVisuals.SettingChanged += (x, _) =>
                    {
                        renderer.enabled = !disableFireworksVisuals.Value;
                    };
                    renderer.enabled = !disableFireworksVisuals.Value;
                }

                Logger.LogInfo("Firework visual effects disabled successfully");
            };

            // 使用 Addressables 加载 AtG 导弹视觉预制体
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileGhost.prefab").Completed += handle =>
            {
                if (handle.Result == null)
                {
                    Logger.LogError("Failed to load MissileGhost.prefab");
                    return;
                }

                GameObject ghostPrefab = handle.Result;

                // 禁用拖尾渲染器
                TrailRenderer trail = ghostPrefab.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    disableAtGMissileVisuals.SettingChanged += (x, _) =>
                    {
                        trail.enabled = !disableAtGMissileVisuals.Value;
                    };
                    trail.enabled = !disableAtGMissileVisuals.Value;
                }

                // 禁用粒子系统
                ParticleSystem[] particles = ghostPrefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    disableAtGMissileVisuals.SettingChanged += (x, _) =>
                    {
                        if (disableAtGMissileVisuals.Value)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                    };
                    if (disableAtGMissileVisuals.Value)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                // 禁用所有渲染器（模型）
                Renderer[] renderers = ghostPrefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    disableAtGMissileVisuals.SettingChanged += (x, _) =>
                    {
                        renderer.enabled = !disableAtGMissileVisuals.Value;
                    };
                    renderer.enabled = !disableAtGMissileVisuals.Value;
                }

                Logger.LogInfo("AtG Missile visual effects disabled successfully");
            };

            // 使用 Addressables 加载烟花道具发射器特效
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkLauncher.prefab").Completed += handle =>
            {
                if (handle.Result == null)
                {
                    Logger.LogWarning("Failed to load FireworkLauncher.prefab");
                    return;
                }

                GameObject launcherPrefab = handle.Result;

                // 禁用粒子系统
                ParticleSystem[] particles = launcherPrefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    disableFireworkActivationEffect.SettingChanged += (x, _) =>
                    {
                        if (disableFireworkActivationEffect.Value)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                        else
                        {
                            ps.Play();
                        }
                    };
                    if (disableFireworkActivationEffect.Value)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                // 禁用所有渲染器
                Renderer[] renderers = launcherPrefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    disableFireworkActivationEffect.SettingChanged += (x, _) =>
                    {
                        renderer.enabled = !disableFireworkActivationEffect.Value;
                    };
                    renderer.enabled = !disableFireworkActivationEffect.Value;
                }

                // 禁用光效
                Light[] lights = launcherPrefab.GetComponentsInChildren<Light>(true);
                foreach (Light light in lights)
                {
                    disableFireworkActivationEffect.SettingChanged += (x, _) =>
                    {
                        light.enabled = !disableFireworkActivationEffect.Value;
                    };
                    light.enabled = !disableFireworkActivationEffect.Value;
                }

                Logger.LogInfo("Firework launcher effect disabled successfully");
            };

            // 使用 Addressables 加载烟花爆炸特效
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkExplosion2.prefab").Completed += handle =>
            {
                if (handle.Result == null)
                {
                    Logger.LogWarning("Failed to load FireworkExplosion2.prefab");
                    return;
                }

                GameObject explosionPrefab = handle.Result;

                // 禁用粒子系统
                ParticleSystem[] particles = explosionPrefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    disableFireworkActivationEffect.SettingChanged += (x, _) =>
                    {
                        if (disableFireworkActivationEffect.Value)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                        else
                        {
                            ps.Play();
                        }
                    };
                    if (disableFireworkActivationEffect.Value)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                // 禁用所有渲染器
                Renderer[] renderers = explosionPrefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    disableFireworkActivationEffect.SettingChanged += (x, _) =>
                    {
                        renderer.enabled = !disableFireworkActivationEffect.Value;
                    };
                    renderer.enabled = !disableFireworkActivationEffect.Value;
                }

                Logger.LogInfo("Firework explosion effect disabled successfully");
            };

            Logger.LogInfo("Plugin " + PluginName + " is loaded!");
        }
    }
}
