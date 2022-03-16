# Descripción

La idea principal detrás de este proyecto es hacer un motor de búsquedas en un conjunto de documentos,
similares a lo que hacen motores como `Google` de los cuales no se tomó ninguna inspiración🙂.

## Algoritmo de búsqueda

Como base para el motor de búsqueda se usa el `model de espacio vectorial`, este es un modelo algebraico utilizado para filtrado, recuperación, indexado y cálculo de relevancia de información. Representa documentos en lenguaje natural de una manera formal mediante el uso de vectores (de identificadores, por ejemplo términos de búsqueda) en un espacio lineal multidimensional(Wikipedia).

## Operadores

Para hacer el motor un poco más funcional se implementaron algunos operadores que permiten, filtrar aún más la información buscada, tales son:

- Exclusión: Este operador indica que cualquier documento que lo contenga delante (ej: `!cuba`) no debe aparecer en los resultados de la búsqueda.

- Inclusión: Análogo al operador anterior, el operador de inclusión indica que todo documento que lo contenga (ej: `^hermosa`) debe aparecer en los documentos encontrados.

- Relevancia: El operador de relevancia cuando se encuentra presente, hace que la palabra deseada tenga más relevancia sobre el resto, notar que esta es acumulativa y se puede aplicar en varios términos de la búsqueda
  (ej: `**blockchain y *bitcoin`).

- Cercanía: Por último tenemos el operador de cercanía, este indica que lo términos buscados deben aparecer cercanos en el documento, esto es mientras más cerca aparezcan en un documento, más relevante será este de cara al resultado esperado, este operador acepta dos o más términos (ej: `blockhain~dinero~estafa`).

## Sugerencia

Como se supone que este motor será usado por personas y a pesar de que suene díficil de creer estas se equivocan, se ha implementado un algoritmo de sugerencia que, adivinen que, `sugiere` una búsqueda más exacta en dependencia de sí la búsqueda se escribión con error, es decir para la palabra `cuva` el algoritmo sugiere `cuba`

## Snipets

Para entrar en contexto, los resultados de búsqueda cuentan con snippets que son pequeños fragmentos de los documentos encontrados conteniendo una o varia de las palabras buscadas en caso de estar estas cercanas

## Raíces y Sinónimos

Como el lenguajes es tan rico y variado se incluyen en las búsquedas documentos en los cuales pueden aparecer tanto las palabras buscadas o bien sus sinónimos o raíces, por ejemplo dada la búsqueda `inteligentemente` los documentos que contengan `inteligencia` también serán devueltos, de manera similar ocurre con los sinónimos (ej: `perro` `can`)

## Ejecutando el proyecto

### Dependencias

. .NET Core 6.0(necesaria)
. Git (recomendable)
. Make (3.1 o posterior)

Primero deberás clonar este repo en github y como no, darle un strella 'https://github.com/theGitNoob/Moogle'
una vez hecho esto nos movemos al directorio que contiene el proyecto y ejecutamos el siguiente comando:

```bash
make dev
```

Si le asignas la variable de entorno `CONTENT_PATH` la aplicación buscará en la ruta asignada sino, buscará por defecto en la carpeta `Content` que se encuentra en la raíz del proyecto

## Implementación

Primero hacer notar que el proyecto cuenta con 4 bibliotecas de clases principales:

- `MoogleServer` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.

- `MoogleEngine` este contiene la lógica a seguir durante la ejecución del algoritmo, en este por ejemplo se
  realiza el indexado de la base de datos.

- `DocumentModel` en esta clase se encuentra todo lo relativo al trabajo con los documentos y el modelaje de estos como vectores, también contiene clases con utilidades relativas al trabajo con este.

- `Stemmer` por último esta clases se encarga de de hacerle stemming a las palabras usando el algoritmo `Snowball` (para más información 'https://snowballstem.org/').

### Flujo de la aplicación

#### Prepocesamiento

Durante el preprocesamiento también se llama al método `BuildDic` de la clase `SynonomusDB` el cual construye un base de datos con los sinónimos de las palabras.

Lo primero a la hora de iniciar la aplicación es el indexado de documentos llamando al método `StartIndex` de la clases `MoogleEngine`, este primero leerá uno a uno los documentos(`*.txt`) y procede a crear una nueva instancia de la clase documento, aquí se procede a guardad las palabras originales y sus posiciones para su posterio uso, luego se eliminan carácteres no alfanúmericos y se convierten a minusculas, una vez hecho esto se guarda la frecuencia de las palabras y de sus raíces además se calcula su TF para cada uno de los términos.
Una vez hecho esto el documento creado será agregado a la colección estática de documentos y terminados todos los documentos se procede a calcular el IDF de estos. Una vez hecho esto la aplicación está lista para procesar las búsquedas de sus usuarios.

#### Búsqueda

Al hacerse una búsqueda se tokeniza la query mediante los mismos métodos usados para tokenizar los documentos, aquí también se guarda la frecuencia de los términos en la query y además se separan cada uno de los operadores usados en la búsqueda. Al terminar se llama al método `GetResults` el cual devuelve los documentos que se relacionan con la búsqueda junto con su respectivo `score` y `snippet`. Una vez obtenido el resultado se procede a hacer la sugerencia de búsqueda.

##### Implementación de los operadores

Para el operador de inclusión y exclusión se incluyen y excluyen los documentos que contengan los términos a excluir.

Para el operador de relevancia se multiplica su score por la relevancia de dicha palabra, esto es, la palabra `****perro` tiene relevancia 4 por tantu su score será multiplicado por dicha cantidad.

El operador de cercanía implementa detrás la idea del algoritmo `sliding window` el cual busca la mínima ventana que contiene a todos los términos involucrados en este operador

##### Snippet

A la hora de buscar los snippet de los documentos se trata de hallar una ventana de tamaño no mayor a 20 que contenga la mayor cantidad posible de términos de la búsqueda.

##### Algoritmo de Sugerencia

El algoritmo de sugerencia consta de dos partes, primero se busca la palabra más cercana a cada uno de los términos, por cercanas se entiende que se tengas que hacer la mínima cantidad de cambios para transformar una en otra, esto se hace mediante el algoritmo de `Levenshtein`('https://en.wikipedia.org/wiki/Levenshtein_distance') terminado este proceso se sustituyen los términos de la query por los sugeridos y si estos aparecen más veces que los términos originales y los términos aparecen en menos de 5 documentos.
