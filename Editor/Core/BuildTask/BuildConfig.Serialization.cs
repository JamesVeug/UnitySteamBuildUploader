using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wireframe
{
    internal partial class BuildConfig
    {
        public Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["enabled"] = Enabled,
                ["guid"] = GUID,
                ["sources"] = m_buildSources.Select(a => a.Serialize()).ToList(),
                ["modifiers"] = m_modifiers.Select(a => a.Serialize()).ToList(),
                ["destinations"] = m_buildDestinations.Select(a => a.Serialize()).ToList(),
            };

            return data;
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            // Enabled
            if (data.TryGetValue("enabled", out object enabled))
            {
                Enabled = (bool)enabled;
            }

            // GUID
            if (data.TryGetValue("guid", out object guid))
            {
                GUID = (string)guid;
            }
            else
            {
                // Generate a new GUID (Introduced in v2.1.0
                GUID = Guid.NewGuid().ToString().Substring(0, 5);
            }

            m_buildSources = new List<SourceData>();
            m_buildDestinations = new List<DestinationData>();

            // Migrate any old data
            try
            {
                Migrate_v120_to_v130(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Sources
            try
            {
                if (data.TryGetValue("sources", out object sources))
                {
                    List<object> sourceList = (List<object>)sources;
                    foreach (object source in sourceList)
                    {
                        Dictionary<string, object> sourceDictionary = (Dictionary<string, object>)source;
                        SourceData sourceData = new SourceData();
                        sourceData.Deserialize(sourceDictionary);
                        m_buildSources.Add(sourceData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Modifiers
            try
            {
                if (data.TryGetValue("modifiers", out object modifiers) && modifiers != null)
                {
                    m_modifiers = new List<ABuildConfigModifer>(); // Clear so we know its empty

                    List<object> modifierList = (List<object>)modifiers;
                    foreach (object modifier in modifierList)
                    {
                        Dictionary<string, object> modifierDictionary = (Dictionary<string, object>)modifier;
                        if (modifierDictionary.TryGetValue("$type", out object modifierType))
                        {
                            Type type = Type.GetType((string)modifierType);
                            if (type != null)
                            {
                                ABuildConfigModifer buildConfigModifer =
                                    Activator.CreateInstance(type) as ABuildConfigModifer;
                                if (buildConfigModifer != null)
                                {
                                    buildConfigModifer.Initialize(() => m_window.Repaint());
                                    buildConfigModifer.Deserialize(modifierDictionary);
                                    m_modifiers.Add(buildConfigModifer);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Destinations
            try
            {
                if (data.TryGetValue("destinations", out object destinations))
                {
                    List<object> destinationList = (List<object>)destinations;
                    foreach (object destination in destinationList)
                    {
                        Dictionary<string, object> destinationDictionary = (Dictionary<string, object>)destination;
                        DestinationData destinationData = new DestinationData();
                        destinationData.Deserialize(destinationDictionary);
                        m_buildDestinations.Add(destinationData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// v1.3.0 adds multiple sources and destinations were introduced along with their Data wrapper types
        /// </summary>
        private void Migrate_v120_to_v130(Dictionary<string, object> data)
        {
            // Source
            if (data.TryGetValue("sourceFullType", out object sourceFullType) && sourceFullType != null)
            {
                string sourceFullPath = (string)sourceFullType;
                Type sourceType = Type.GetType(sourceFullPath);
                if (sourceType != null)
                {
                    SourceData sourceData = new SourceData();
                    ABuildSource source =
                        Activator.CreateInstance(sourceType, new object[] { m_window }) as ABuildSource;
                    if (source != null)
                    {
                        // Source
                        Dictionary<string, object> sourceDictionary = (Dictionary<string, object>)data["source"];
                        source.Deserialize(sourceDictionary);
                        sourceData.Source = source;
                        sourceData.SourceType = UIHelpers.SourcesPopup.Values.FirstOrDefault(a => a.Type == sourceType);
                        m_buildSources.Add(sourceData);
                    }
                }
            }

            // Destination
            if (data.TryGetValue("destinationFullType", out object destinationFullType) && destinationFullType != null)
            {
                string destinationFullPath = (string)destinationFullType;
                Type type = Type.GetType(destinationFullPath);
                if (type != null)
                {
                    DestinationData destinationData = new DestinationData();
                    destinationData.Destination =
                        Activator.CreateInstance(type, new object[] { m_window }) as ABuildDestination;
                    if (destinationData.Destination != null)
                    {
                        Dictionary<string, object> destinationDictionary =
                            (Dictionary<string, object>)data["destination"];
                        destinationData.Destination.Deserialize(destinationDictionary);
                        destinationData.DestinationType =
                            UIHelpers.DestinationsPopup.Values.FirstOrDefault(a => a.Type == type);
                        m_buildDestinations.Add(destinationData);
                    }
                }
            }
        }
    }
}