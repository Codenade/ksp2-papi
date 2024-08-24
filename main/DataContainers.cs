using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ksp2_papi
{
    public class ConfigData
    {
        public List<PapiData> PapiData { get; set; }
    }

    public struct Vector3Data
    {
        [JsonProperty(propertyName: "x")]
        public float X { get; set; }
        [JsonProperty(propertyName: "y")]
        public float Y { get; set; }
        [JsonProperty(propertyName: "z")]
        public float Z { get; set; }

        public static implicit operator Vector3(Vector3Data v3d) => new Vector3(v3d.X, v3d.Y, v3d.Z);
    }

    public class PapiData
    {
        public string ID { get; set; }
        public string ParentName { get; set; }
        public Vector3Data LocalPosition { get; set; }
        public Vector3Data LocalRotation { get; set; }
    }
}
