Implements a subset de **Markdown**.

How to use it
=============

    var html = MkDown.Encode(text);

Negrita
=======

	**importante**
	genera:
	<strong>importante</strong>

Itálica
=======

	__importante__
	genera:
	<i>importante</i>

Listas
=======
Empezar la línea con 2 espacios, asterísco y un espacio.

	* Ir a la compra
	* Ir al gimnasio
    
	genera:

	<ul>
	<li>Ir a la compra</li>
	<li>Ir al gimnasio</li>
	</ul>


Títulos
=======

	título 1
	========
	genera:
	<h1>título 1</h1>

	Subtítulo 1
	--------
	genera:
	<h2>Subtítulo 1</h2>


Links
=======

	[](www.google.com)
	genera:
	<a href='www.google.com'>www.google.com</a> 

	[Página de Google](www.google.com)
	genera:
	<a href='www.google.com'>Página de Google</a> 

Imágenes
=======

	![](www.google.com/logo.gif)
	genera:
	<img src='www.google.com/logo.gif' /> 

	![Texto alternativo](www.google.com/logo.gif)
	genera:
	<img src='www.google.com/logo.gif' alt='Texto alternativo' /> 
