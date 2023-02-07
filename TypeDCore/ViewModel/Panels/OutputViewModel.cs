﻿using System;
using System.Windows;
using TypeD.Helpers;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Panels
{
    internal class OutputViewModel : ViewModelBase
    {
        // Models
        ILogModel LogModel { get; set; }

        // Constructors
        public OutputViewModel(FrameworkElement element) : base(element)
        {
            LogModel = ResourceModel.Get<ILogModel>();
            LogModel.AttachLogOutput("OutputView", (message) =>
            {
                OnCMDOutput(message);
            });
            CMD.Output += OnCMDOutput;
        }

        // Functions
        private void OnCMDOutput(string output)
        {
            OutputText += output + Environment.NewLine;
        }

        public void Unload()
        {
            LogModel.DetachLogOutput("OutputView");
            CMD.Output -= OnCMDOutput;
        }

        // Properties
        private string _outputText;
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }
    }
}
