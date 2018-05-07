# Prism.Logging.NLog
An extension class for implementing NLog with your Prism WPF MVVM app

## Introduction
It's an extension for Prism that allows you to easily use NLog logging library as your Prism WPF app logger

## Quick Start
First, install NLog on your app project then create and set up its config file (NLog.config), after that add the Prism.Logging.NLog logger in 
your Prism Bootstrapper class:

```csharp
using Prism.Logging.NLog;
using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Unity;
using System;
using System.Windows;

namespace ForexNewsStation.Core
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Fields

        private Logger _logger;

        #endregion Fields

        #region Constructors

        public Bootstrapper()
        {
            // The "Internal.log" is path of internal logging file
            // If internal logging is not enabled on your NLog config file then use the 
            // blank constructor
            _logger = new Logger("Internal.log");
        }

        #endregion Constructors

        #region Methods

        protected override ILoggerFacade CreateLogger()
        {
            return _logger;
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Views.ShellView>();
        }

        protected override void InitializeShell()
        {
            if (Application.Current.MainWindow != null) Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // Here you register the logger instance to the container so you will be able to get it in your view model constructors
            Container.RegisterInstance(_logger);
        }

        #endregion Methods
    }
}
```

Now you can access the logger instance in your view model constructors:

```csharp
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Prism.Logging.NLog;

namespace LogApp.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Fields

        private IEventAggregator _eventAggregator;

        private IRegionManager _regionManager;

        private Logger _logger;

        #endregion Fields

        #region Constructor

        public ShellViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _regionManager = regionManager;

            _eventAggregator = eventAggregator;

            // Here is the logger
            _logger = logger;

            // You can subscribe to OnException event if you are using the LogException method of Logger
      // (Same for OnError event and LogError method)
            _logger.OnException += Logger_OnException;
        }

        #endregion Constructor

        #region Methods

        private void Logger_OnException(Exception ex, string exText)
        {
        }
        
        #endregion Methods
    }
}
```

You can access the NLog.Logger via NLogger property of Logger class:

```csharp
// Using NLog logger Trace method
_logger.NLogger.Trace("Logging on trace");
// Or (Other overloads of NLogger isn't implemented so you have to use _logger.NLogger.Trace(**))
_logger.Trace("Logging on trace");
```

To log an exception that will trigger OnException event of Logger class use LogException method:

```csharp
// It will generate a text detail of exception and write it to "Fatal"
_logger.LogException(exception);
```
