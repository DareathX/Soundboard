using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Soundboard.Settings
{
    public class Config
    {
        public float FirstVolume { get; set; }
        public double FirstVolumeAngle { get; set; }
        public string FirstVolumePercentage { get; set; }
        public float SecondVolume { get; set; }
        public double SecondVolumeAngle { get; set; }
        public string SecondVolumePercentage { get; set; }
        public List<Sound.Files> SavedSoundFiles { get; set; }


        public void Save()
        {
            using (FileStream fileStream = new FileStream("Config.xml", FileMode.Create))
            {
                XmlSerializer xml = new XmlSerializer(typeof(Config));
                xml.Serialize(fileStream, this);
            }
        }
    }
}
