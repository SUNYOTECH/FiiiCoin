   M S B A M L   `   `   `  ���� N  JFiiiCoin.Wallet.Win, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null[2clr-namespace:FiiiCoin.Wallet.Win.Views.ShellPages$FiiiCoin.Wallet.Win.Views.ShellPages  e7clr-namespace:FiiiCoin.Wallet.Win.ViewModels.ShellPages)FiiiCoin.Wallet.Win.ViewModels.ShellPages  W0clr-namespace:FiiiCoin.Wallet.Win.CustomControls"FiiiCoin.Wallet.Win.CustomControls  4    .FiiiCoin.Wallet.Win.Views.ShellPages.ImagePage   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     Dlocal2clr-namespace:FiiiCoin.Wallet.Win.Views.ShellPages     Fvm7clr-namespace:FiiiCoin.Wallet.Win.ViewModels.ShellPages     Kcustomcontrols0clr-namespace:FiiiCoin.Wallet.Win.CustomControls     $	��680��$	��350��  N� Title$  	ImagePage�� � DataContext >   8FiiiCoin.Wallet.Win.ViewModels.ShellPages.ImageViewModel  .��� 
������� � RowDefinitions �� $��40 ��� $	��257 ��� $��* �.+�7   1FiiiCoin.Wallet.Win.Views.ShellPages.TitleControl     RequestPayPage_title   Title# �   �� $��1��$��Center=�.+��� $	��140��$	��140������ *
QrCodePath+�� $��0 20 0 0q�$��Center=����� *	Address+�� $��2��$��0 1 0 0q����#E5E5E5$
��20 0q�._��  �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ���  � ColumnDefinitions �� $
��Auto ��� .+��� $��5��$��2�� �� Orientation$ 
Horizontal=�.+�3   -FiiiCoin.Wallet.Win.CustomControls.PathButton    path_btn_copyUri   PathData# [ $��6q�$
��Auto�� ! btn_white_normal_ContentColor   DefaultFillBrush# [  $  btn_white_mouseOver_ContentColor   MouseOverBrush# [  ! btn_white_enable_ContentColor	   IsPressedBrush#	 [ 
   IsEnabledBrush#
 [   CopyAddress#���  ���#D4D4D4
�������$��1q����#8C8C8C$��32��$��2q�$��90������ *CopyAddressCommand+    path_btn_copyimage# [ $��6q�$
��Auto��# [ # [ #	 [ #
 [   	SaveImage#���  ���#D4D4D4
�������$��1q����#8C8C8C$��32��$��2q�$��90������ *SaveImageCommand+�� $��5��$��2�����#D4D4D4
�������$��1q����#8C8C8C 	 Close#���  $��32��$��90��$��Right=����� *ClosePopupCommand+