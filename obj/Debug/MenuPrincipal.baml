   M S B A M L   `   `   `  ����N  JProyectoFinalAmaury, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:ProyectoFinalAmauryProyectoFinalAmaury  5   	   '    !ProyectoFinalAmaury.MenuPrincipal   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:ProyectoFinalAmaury     5   	     � Title$   ANALISIS LEXICO SINTACTICO��5   	   $	��600��6,   $��933.598��69    � 
ResizeMode$ NoResize=�6I   .��5	      � .+�5
   
   �� -   5         ��Name$ CanvasPrincipal��5
      $
��Left=�6*   $	��571��6E   $	��Top=�6R   $	��928��6j   ���	#FF555555.+�5      �� -   5         ��Name$ ImportarBoton��5   -   $��IMPORTAR CODIGO��6D    �� Left$	 427��6^   
 �� Top$ 53��6p   $	��478��6�   $
��Bold"�6�   $��39��6�   �� 5      -   5         ��Name$ 
CajaLexico��5      $	��190��6C   $	 427��6P    �� TextWrapping$
 Wrap=�6b   $��ANALEX��6v   $ 97��6�   $	��478��6�   ��. " �� VerticalScrollBarVisibility$ Visible=�6�   $	 �� HorizontalScrollBarVisibility$	 Visible=�6�   
 �� AcceptsReturn
 . 
���   �
������� �� 
IsReadOnly . 6N  �� 5      -   5        $ CajaSintactico��5      $	��190��6G   $	 427��6T   $
 Wrap=�6f   $��ANASIN��6z   $	 359��6�   $	��478��6�   ��. $ Visible=�6�   $	 Visible=�6�   
 . 
���   �
������� . 6S  �� 5      -   5         ��Name$ ScrollCajaCodigo��5      $	��539��65   $ 48��6B   $ 10��6S   $	��363��6c   .��5      �� -   5        $ 
CajaCodigo��5      $
��Auto��6.   $
 Wrap=�6<   $�� ��6P   $
��Auto��6X   
 .  �� 
AcceptsTab . 
���   �
������� . ��6�   Z� 6�   5      �� 5      -   5        $ ScrollLineador��5      $	��539��63   $ 10��6@   $ 10��6Q   $��38��6a   $��Hidden=�6l   .��5      �� -   5        $ Lineador��5      $
��Auto��6,   $
 Wrap=�6:   $�� ��6N   $
��Auto��6V   
 . ���	#FF1D1D1D
������� �� TextAlignment$ Center=�6�    . ��6�   Z� 6�   5      �� 5      -	   5        $ ANALISIS_SINTACTICO_BOTON��5   .   {�.  $��ANALIZAR SINTAXIS��6c   $
��Bold"�6   $��43��6�   $	 765��6�   $	 301��6�   $	��140��6�   �� 5      -
   5        $ ANALISIS_LEXICO_BOTON��5   ,   {�.  $��ANALIZAR LEXICO��6]   $
��Bold"�6w   $	 427��6�   $	 301��6�   $��43��6�   $	��140��6�   �� 5      -   5         ��Name$	 Por��5      $S��MHecho por Amaury Morales Cerecedo para la asignatura Teoria de la Computacion��6!   $	 427��6y   $	��478��6�   $��43��6�   $ 10��6�   
�������$
��Bold"�6�   $��Italic$�6�   5      5      5      