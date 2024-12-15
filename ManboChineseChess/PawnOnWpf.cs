using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LibChineseChess;

namespace ManboChineseChess
{
    public class PawnOnWpf : DependencyObject
    {
        public Pawn Pawn
        {
            get { return (Pawn)GetValue(PawnProperty); }
            set { SetValue(PawnProperty, value); }
        }

        public double LocationX
        {
            get { return (double)GetValue(LocationXProperty); }
            set { SetValue(LocationXProperty, value); }
        }

        public double LocationY
        {
            get { return (double)GetValue(LocationYProperty); }
            set { SetValue(LocationYProperty, value); }
        }

        public double BoardWidth
        {
            get { return (double)GetValue(BoardWidthProperty); }
            set { SetValue(BoardWidthProperty, value); }
        }

        public double BoardHeight
        {
            get { return (double)GetValue(BoardHeightProperty); }
            set { SetValue(BoardHeightProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }


        public double ActualX => (double)GetValue(ActualXPropertyKey.DependencyProperty);
        public double ActualY => (double)GetValue(ActualYPropertyKey.DependencyProperty);


        public static readonly DependencyProperty PawnProperty =
            DependencyProperty.Register(nameof(Pawn), typeof(Pawn), typeof(PawnOnWpf), new PropertyMetadata(default(Pawn)));

        public static readonly DependencyProperty LocationXProperty =
            DependencyProperty.Register(nameof(LocationX), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(0.0, propertyChangedCallback: OnXPropertyChanged));

        public static readonly DependencyProperty LocationYProperty =
            DependencyProperty.Register(nameof(LocationY), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(0.0, propertyChangedCallback: OnYPropertyChanged));

        public static readonly DependencyProperty BoardWidthProperty =
            DependencyProperty.Register(nameof(BoardWidth), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(800.0));

        public static readonly DependencyProperty BoardHeightProperty =
            DependencyProperty.Register(nameof(BoardHeight), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(900.0));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(PawnOnWpf), new PropertyMetadata(false));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(PawnOnWpf), new PropertyMetadata(true));

        public static readonly DependencyPropertyKey ActualXPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ActualX), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(0.0));

        public static readonly DependencyPropertyKey ActualYPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ActualY), typeof(double), typeof(PawnOnWpf), new PropertyMetadata(0.0));






        private static void OnXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PawnOnWpf pawnOnWpf)
            {
                return;
            }

            d.SetValue(ActualXPropertyKey, pawnOnWpf.LocationX / 8 * pawnOnWpf.BoardWidth);
        }

        private static void OnYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PawnOnWpf pawnOnWpf)
            {
                return;
            }

            d.SetValue(ActualYPropertyKey, pawnOnWpf.LocationY / 9 * pawnOnWpf.BoardHeight);
        }
    }
}
