# Moogle

![project logo](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

Moogle! es una aplicación _totalmente original_ cuyo propósito es buscar inteligentemente un texto en un conjunto de documentos.

Es una aplicación web, desarrollada con tecnología .NET Core 6.0, específicamente usando Blazor como _framework_ web para la interfaz gráfica, y en el lenguaje C#.
La aplicación está dividida en dos componentes fundamentales:

- `MoogleServer` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.
- `MoogleEngine` es una biblioteca de clases donde está... ehem... casi implementada la lógica del algoritmo de búsqueda.
- `MoogleServer` es una biblioteca de clases que implementa la loǵica detrás de la manipulacion de los documentos

## Algoritmo de Búsqueda

El algoritmo de búsqueda está basado en el modelo de espacio vectorial, el cual modela cada documento y las queries en vectores donde las componentes de esos vectores son el tf-idf de cada término, esta manera de modelar los documentos y las queries permite conocidos dos vectores saber el grado de similitud entre ellos calculando el coseno del ángulo que se forma entre ellos.

## TF_IDF

Al modelar los modelos como vectores, le asigamos como valor a cada componente el tf_idf asociado, el `TF` se usa para saber la frequencia de un termino en cada uno de los documentos y el `IDF` se encarga de que los términos que aparecen en más documentos sean menos relevantes.

## Caracteristicas

Como todo todo motor de busqueda respetable😂, Moogle cuenta con diversas caracteristicas para hacerlo mas útil de cara al usuario, entre ellas se encuentran distintos operadores para hacer las busquedas más precisas, la inclusion en los resultados de búsqueda de palabras que tienen la misma raíz o el mismo significado, en caso de errores al escribir la palabra también se muestra una sugerencia de la posible palabra correcta.

### Operadores

**Operador de Cercannía**

![project logo](cercania.png)

- El operador de cercanía tiene por objetivo aumentar la relevancia de un documento en dependencia de que tan cercanas esten dichas palabras en el documento(Notar que se puede usar sobre dos o mas palabras)

**Operador de Relevancia**

![project logo](relevancia.png)

- El operador de relevancia aumenta la importancia de la(s) palabra(s), notar que este es acumulativo y mientras.

**Operador de Exclusión**

- El símbolo `!` delante de una palabra (e.j., `"harry !potter"`) indica que la palabra `potter` no aparecerá en ningún documento que sea devuelto.

**Operador de Inclusión**

- El símbolo `^` delante de una palabra (e.j., `"harry ^potter"`) indica que dicha palabra estará presente en cualquier documento que sea devuelto.

### Sinónimos y Raíces

- Para hacer las búsquedas un poco más flexibles se incluyen en los resultados de estas sus sinónimos y raíces aunque con una menor relevancia.
