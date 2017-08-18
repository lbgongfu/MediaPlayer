﻿using System.ComponentModel;
using Android.Media;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Android.App;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarians.MediaPlayer.VideoPlayer), typeof(Xamarians.MediaPlayer.Droid.VideoPlayerRenderer))]
namespace Xamarians.MediaPlayer.Droid
{

    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, RelativeLayout>, INativePlayer
    {
        VideoView _videoView;
        ProgressBar progressBar;
        MediaController mediaController;
        Button fullScreenBtn;
        bool _prepared;
        static Activity _context;

        public static void Init(Activity context)
        {
            _context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;
            var context = Xamarin.Forms.Forms.Context;

            // Set Native Control
            var relativeLayout = new RelativeLayout(context);
            relativeLayout.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            relativeLayout.SetPadding(0, 0, 0, 0);
            relativeLayout.SetBackgroundColor(Android.Graphics.Color.Black);
            SetNativeControl(relativeLayout);
            Element.SetNativeContext(this);

            // Create Video View
            InitVideoView();

            // Start the MediaController
            InitMediaController();

            // Show progressbar
            InitProgressBar();
            SetSource();
        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (VideoPlayer.SourceProperty.PropertyName.Equals(e.PropertyName))
            {
                SetSource();
            }
        }

        #region Private Methods

        private void InitVideoView()
        {
            _videoView = new VideoView(Context);
            _videoView.Holder.SetKeepScreenOn(true);
            _videoView.Prepared += _videoView_Prepared;
            _videoView.Error += _videoView_Error;
            _videoView.Completion += _videoView_Completion;
            _videoView.Info += _videoView_Info;

            var lv = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            lv.AddRule(LayoutRules.CenterInParent);
            _videoView.LayoutParameters = lv;
            Control.AddView(_videoView);
        }

        private void InitMediaController()
        {
            mediaController = new MediaController(Context, false);
            mediaController.SetAnchorView(_videoView);
            _videoView.SetMediaController(mediaController);
        }
        private void InitProgressBar()
        {
            progressBar = new ProgressBar(Context);
            progressBar.Indeterminate = false;
            progressBar.Visibility = Android.Views.ViewStates.Invisible;
            var lparams = new RelativeLayout.LayoutParams(100, 100);
            lparams.AddRule(LayoutRules.CenterInParent);
            Control.AddView(progressBar, lparams);
        }

        public void SetScreen(bool isPortrait)
        {
            if (_context == null)
                return;
           // _context.Window.AddFlags(WindowManagerFlags.Fullscreen);

            if (isPortrait)
                FullScreen();
            else
                ExitFullScreen();
        }

        public void FullScreen()
        {
            _context.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            var window = (Context as Activity).Window;

            window.DecorView.SystemUiVisibility =
              (Android.Views.StatusBarVisibility)Android.Views.SystemUiFlags.LayoutStable
              | (Android.Views.StatusBarVisibility)Android.Views.SystemUiFlags.LayoutFullscreen
              | (Android.Views.StatusBarVisibility)Android.Views.SystemUiFlags.Fullscreen
              | (Android.Views.StatusBarVisibility)Android.Views.SystemUiFlags.ImmersiveSticky
              ;
        }

        public void ExitFullScreen()
        {
            _context.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            var window = (Context as Activity).Window;
            window.DecorView.SystemUiVisibility = (Android.Views.StatusBarVisibility)Android.Views.SystemUiFlags.Visible;
        }


        private void SetSource()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Element.Source))
                    return;
                _prepared = false;
                progressBar.Visibility = Android.Views.ViewStates.Visible;
                _videoView.SetVideoURI(Android.Net.Uri.Parse(Element.Source));
                _videoView.RequestFocus();
            }
            catch (Java.Lang.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                Element.OnError(e.Message);
            }
        }

        #endregion

        #region INativePlayer

        public int Duration
        {
            get
            {
                return _prepared ? _videoView.Duration : 0;
            }
        }

        public int CurrentPosition
        {
            get
            {
                return _prepared ? _videoView.CurrentPosition : 0;
            }
        }

        public bool IsNativeControlsVisible
        {
            get
            {
                if (mediaController == null)
                    return false;
                return mediaController.IsShown;
            }
        }


        public void Play()
        {
            if (!_prepared) return;
            _videoView.Start();
        }

        public void Pause()
        {
            if (!_prepared) return;
            if (_videoView.CanPause())
                _videoView.Pause();
        }

        public void Stop()
        {
            if (!_prepared) return;
            _videoView.StopPlayback();
        }

        public void Seek(int seconds)
        {
            if (!_prepared) return;
            _videoView.SeekTo(seconds * 1000);
        }

        #endregion

        #region Events

        private void _videoView_Prepared(object sender, System.EventArgs e)
        {
            progressBar.Visibility = Android.Views.ViewStates.Invisible;
            _prepared = true;
            if (Element.AutoPlay)
                Play();
            Element?.OnPrepare();
        }

        private void _videoView_Info(object sender, Android.Media.MediaPlayer.InfoEventArgs e)
        {
            progressBar.Visibility = e.What == MediaInfo.BufferingStart ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Invisible;
        }

        private void _videoView_Completion(object sender, System.EventArgs e)
        {
            Element?.OnCompletion();
        }

        private void _videoView_Error(object sender, Android.Media.MediaPlayer.ErrorEventArgs e)
        {
            Element?.OnError(e.What.ToString());
        }

        public void HidePlayerController(bool isHide = true)
        {
            if (mediaController != null)
            {
                mediaController.Hide();
            }
        }



        #endregion

    }
}