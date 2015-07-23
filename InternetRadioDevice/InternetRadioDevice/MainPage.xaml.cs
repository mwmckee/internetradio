﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Networking.Sockets;
using System.Diagnostics;
using Win10_LCD;
using Windows.Devices.Gpio;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.ApplicationModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InternetRadioDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isAsleep;

        private DisplayController displayController;
        private PresetManager presetManager;
        private InputManager inputManager;

        public MainPage()
        {
            isAsleep = false;
            this.InitializeComponent();

            displayController = new DisplayController();
            presetManager = new PresetManager();
            inputManager = new InputManager();
        }

        private async Task startup()
        {
            await displayController.WriteMessageAsync(Config.Messages.StartupMessageLineOne, Config.Messages.StartupMessageLineTwo, 0);

            await Task.Delay(Config.Messages.StartupMessageDelay);
            await playChannel(presetManager.CurrentChannel());
        }

        private async Task shutdown()
        {
            await displayController.WriteMessageAsync(Config.Messages.ShutdownMessageLineOne, Config.Messages.ShutdownMessageLineTwo, 0);
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                  {
                      this.radioPlayer.Pause();
                  });
        }

        private async Task playChannel(Channel channel)
        {
            Debug.WriteLine("Play Channel: " + channel.Name);
            await displayController.WriteMessageAsync("Now Playing: ", channel.Name, 0);
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                this.radioPlayer.Source = channel.Address;
                 this.radioPlayer.Play();
             });

        }

        private async Task changeVolume(double volumeChange)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                 if ((volumeChange > 0 && radioPlayer.Volume < 1) || (volumeChange < 0 && radioPlayer.Volume > 0))
                 {
                     radioPlayer.Volume += volumeChange;
                 }

                 displayController.WriteMessageAsync("Volume: ", ((int)(radioPlayer.Volume * 100)).ToString() + "%", 3);
             });

        }

        private async Task toggleSleep()
        {
            if (isAsleep)
            {
                await startup();
            }
            else
            {
                await shutdown();
            }

            isAsleep = !isAsleep;
        }

        private async void InputManager_InputRecieved(object sender, InputRecievedEventArgs e)
        {
            switch(e.Action)
            {
                case InputAction.AddChannel:
                    var presetData = e.Message.Split(';');
                    presetManager.AddChannel(new Channel(presetData[0], new Uri(presetData[1])));
                    break;
                case InputAction.NextChannel:
                    await playChannel(presetManager.NextChannel());
                    break;
                case InputAction.PreviousChannel:
                    await playChannel(presetManager.PreviousChannel());
                    break;
                case InputAction.VolumeDown:
                    await changeVolume(-0.1);
                    break;
                case InputAction.VolumeUp:
                    await changeVolume(0.1);
                    break;
                case InputAction.Sleep:
                    await toggleSleep();
                    break;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await displayController.Initialize();
            inputManager.InputRecieved += InputManager_InputRecieved;
            await inputManager.Initialize();
            await startup();
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await shutdown();
        }
    }
}