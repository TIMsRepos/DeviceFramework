using System;
using System.IO;
using System.Media;
using System.Windows.Forms;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Extensions;

namespace TIM.Devices.Framework
{
    public class Notifier
    {
        public static void PlayAudio(Enums.AudioSamples MyAudioSample)
        {
            try
            {
                string strPath = string.Format(@"{0}\Media\{1}.wav", Path.GetDirectoryName(Application.ExecutablePath), MyAudioSample.ToString().ToLower());
                using (Stream MyStream = File.OpenRead(strPath))
                {
                    SoundPlayer MyPlayer = new SoundPlayer(MyStream);
                    MyPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                ex.LogWarning();
            }
        }
    }
}