using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GateAccessControl
{
    public class CommandTextBox : TextBox, ICommandSource
    {
        public CommandTextBox() : base()
        {
        }

        #region ICommand Interface Members

        // Make Command a dependency property so it can use databinding.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandTextBox),
                new PropertyMetadata((ICommand)null,
                new PropertyChangedCallback(CommandChanged)));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty ExecutedProperty =
            DependencyProperty.Register(
                "Executed",
                typeof(object),
                typeof(CommandTextBox),
                new PropertyMetadata((object)null));

        public object Executed
        {
            get => (object)GetValue(ExecutedProperty);
            set => SetValue(ExecutedProperty, value);
        }

        public static readonly DependencyProperty CanExecuteProperty =
            DependencyProperty.Register(
                "CanExecute",
                typeof(object),
                typeof(CommandTextBox),
                new PropertyMetadata((object)null));

        public object CanExecute
        {
            get => (object)GetValue(CanExecuteProperty);
            set => SetValue(CanExecuteProperty, value);
        }

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register(
                "CommandTarget",
                typeof(IInputElement),
                typeof(CommandTextBox),
                new PropertyMetadata((IInputElement)null));

        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                "CommandParameter",
                typeof(object),
                typeof(CommandTextBox),
                new PropertyMetadata((object)null));

        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        #endregion ICommand Interface Members

        // Command dependency property change callback.
        private static void CommandChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            CommandTextBox clb = (CommandTextBox)d;
            clb.HookUpCommand((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        // Add a new command to the Command Property.
        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            // If oldCommand is not null, then we need to remove the handlers.
            if (oldCommand != null)
            {
                RemoveCommand(oldCommand, newCommand);
            }
            AddCommand(oldCommand, newCommand);
        }

        // Remove an old command from the Command Property.
        private void RemoveCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }

        // Add the command.
        private void AddCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = new EventHandler(CanExecuteChanged);
            canExecuteChangedHandler = handler;
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
            }
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;

                // If a RoutedCommand.
                if (command != null)
                {
                    if (command.CanExecute(CommandParameter, CommandTarget))
                    {
                        this.IsEnabled = true;
                    }
                    else
                    {
                        this.IsEnabled = false;
                    }
                }
                // If a not RoutedCommand.
                else
                {
                    if (Command.CanExecute(CommandParameter))
                    {
                        this.IsEnabled = true;
                    }
                    else
                    {
                        this.IsEnabled = false;
                    }
                }
            }
        }

        private static EventHandler canExecuteChangedHandler;

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (this.Command != null)
            {
                RoutedCommand command = Command as RoutedCommand;

                if (command != null)
                {
                    command.Execute(CommandParameter, CommandTarget);
                }
                else
                {
                    ((ICommand)Command).Execute(CommandParameter);
                }
            }
        }
    }
}