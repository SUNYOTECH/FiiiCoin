   M S B A M L   `   `   `  ����a  ]PresentationFramework.Luna, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35gJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.LunaMicrosoft.Windows.Themes  5      N JFiiiCoin.Wallet.Win, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:FiiiCoin.Wallet.Win.ModelsFiiiCoin.Wallet.Win.Models 5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source   tradeRecordBorderStyle �� 
TargetType �� Property �� Value3   -System.Windows.Media.Effects.DropShadowEffect    
BlurRadius    Color    Opacity �� TargetProperty  TradeRecordInfoTemplate	 �� DataType0  *FiiiCoin.Wallet.Win.Models.TradeRecordInfo
 ��Name �� Left
 �� Top ! _timestampToDateTimeConverter �� 	Converter �� ConverterParameter  _paymentToMarkConverter ��Name �� Orientation �� Right ��Name  _longToDoubleConverter �� Triggers �� Value �� Binding �� 
TargetName�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      SlunaJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Luna  5      3models(clr-namespace:FiiiCoin.Wallet.Win.Models  5        5      �� 5   
   $T Npack://application:,,,/FiiiCoin.Wallet.Win;component/resources/Converters.xaml?�6   %�  &	        &	 �    7   7  7  7  �� 5
      " ��6+   .�5   
   ��  � ��$	 400��6%   �� 5   
    � ��$	 920��6%   �� 5   
    � ��$	 100��6#   �� 5   
    � ��
 �����6'   �� 5   
    � �� �#F1F7FF6(   �� 5   
    � ��$ 1q�6,   �� 5   
    � ��$ 0 0 0 20q�6#   �� 5   
    �@J�Effect 5         5      OG��$ 20Q�6/   $ #1B76FF��6?   $	 0.1Q�6O   $ 
BlurRadius�5   1   5      5      5      �� 5      "	  63   .R�5   
   �� 8��  ._�5       �� .+�5!      �� $
 image��6   $#��../../images/page/receipt.png��6'   $ 30��6N   $ 22��6_   $��30��6o   $��30��6z   �� 5#      $ 75��6�   $ 22��6�   ��6   �� *Payment.Time+8  $ yyyy-MM-dd HH:mm:ss��6�   �� 5%      $ 75��6h   $ 47��6y   ��6   �� *	Payment+8  6y   �� 5'      $ panel��6   $ 
Horizontal=�6,   $ 22��6E   $ 30��6U   $��Right=�6g   .+�5(      �� $ tb_model��6    $��+ ��62   �� 5)      ��6    �� *Payment.Amount+8  �� 5*      $�� FIII��6    5+      5,      5-       5.   
   �� 5/      $
 self��6?    6   �� *Payment.Category+.a�50      �� $ image��6    � ��$ ../../images/page/pay.png��6>   �� 51      $ tb_model��6    � ��$ - ��6?   �� 52      $ panel��6    � ��
 �  ��6N   53      �� 54      $
 send��6?    6   �� *Payment.Category+.a�55      �� $ image��6    � ��$ ../../images/page/pay.png��6>   �� 56      $ tb_model��6    � ��$ - ��6?   �� 57      $ panel��6    � ��
 �  ��6N   58      5:      5<      