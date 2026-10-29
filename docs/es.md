# Cthangover: Story — el juego principal

Cthangover es una novela visual de humor negro sobre Marao, que despierta en un callejón tras una noche pésima: sin memoria, con un trapo rojo y un libro viejo que no recuerda haber comprado. Este mod es el contenido propio del juego: sus escenas, escenarios, personajes, objetos, misiones, el mapa del mundo, la música y el texto de la historia. Todo lo demás — diálogos, inventario, combates, la pantalla del mapa — lo aportan los mods de motor y de mecánicas que el juego combina.

Se juega desde **Jugar → Cthangover → Nueva partida**.

![La primera escena de una partida nueva: Marao, con un trapo rojo, de pie en un callejón oscuro; la narración describe su resaca. El texto del prólogo todavía está solo en ruso.](assets/opening.jpg)

## En qué consiste el juego

- **El prólogo.** La historia empieza en el callejón y continúa por un cementerio y la orilla del río hasta una villa antigua. El prólogo se juega como una novela: líneas de texto, el retrato de Marao y decisiones.
- **La villa y el pueblo.** Tras el prólogo te mueves entre lugares con decisiones: el exterior de la villa, sus habitaciones (vestíbulo, salón, cocina, dormitorio, taller y sótano), la entrada del pueblo y las afueras.
- **Detalles de supervivencia.** Una partida nueva empieza con carne de lobo y leña en la bolsa; la cocina lleva la cuenta de las raciones y su consumo diario y permite cocinar.
- **Misiones.** El diario sigue las tareas de la historia, empezando por *El despertar* y *La casa vieja*.
- **El mapa del mundo.** El panel del mapa muestra el distrito con los lugares a los que puedes viajar.
- **Combates.** Por defecto se lucha con el combate de cartas; en los ajustes del juego puedes cambiar al combate por órdenes.

![El exterior de la villa al mediodía: una casa de molino junto a una cascada y las opciones "Entrar en la casa", "Al pueblo" y "Al bosque".](assets/villa-outside.jpg)

![La cocina: el contador de raciones arriba a la izquierda y las opciones "Salir" y "Cocinar".](assets/kitchen.jpg)

![El mapa del mundo del distrito norte con los marcadores de la villa y la entrada del pueblo.](assets/world-map.jpg)

![Un combate de cartas en el pueblo: Marao y un compañero contra dos lobos.](assets/battle.jpg)

## Idioma

Los escenarios del prólogo están escritos en ruso y aún no tienen traducción, así que su texto aparece en ruso en cualquier idioma de la interfaz. Las escenas de la villa y el pueblo, el diario de misiones, los objetos y los menús sí están traducidos.

## Cómo se monta el juego

El juego es la colección **Cthangover**. Sus bloques muestran qué hace cada mod:

```text
Historia       cthangover_game, cthangover_murakami, cthangover_party, town_pack
Motor 2D       la carcasa, diálogos, interactivos, tiempo y luz, efectos, mapa del mundo
Personajes     personajes y habilidades
Misiones       el diario
Objetos        inventario, cocina
Combate        plataforma de turnos + combate de cartas (núcleo por defecto)
Combate (FF)   núcleo de combate por órdenes (opcional)
Reclutamiento  reclutar personajes derrotados (opcional)
Live2D         avatares animados (opcional)
Interfaz       kit de UI, menú, audio
Herramientas de desarrollo (opcional)  editores, validador, atlas, gestor de mods
```

- **Cthangover: Murakami** añade una línea argumental aparte, y **Cthangover: Starter Party** mete a Marao en el grupo desde el principio. Forman parte del mismo juego, pero son mods separados.
- **Town Pack** aporta las imágenes de la entrada del pueblo y del vestíbulo, y **Music Pack** la banda sonora. Este mod añade una lista de reproducción propia para la escena de la entrada del pueblo.
- La colección fija `battle.core` en `card_battle`. Puedes cambiarlo, y también los mods de cualquier bloque, en **Mods & Collections → Colecciones → Cthangover → Editar esquema**; tus partidas guardadas se conservan.

## Para autores

El contenido vive en carpetas normales:

```filestree
cthangover_game/
├── scenes/          un JSON por lugar: nombre, fondo, ambiente, escenario por defecto
├── scenarios/       prolog/, home/, town/ y start.scenario
├── characters/      jugador, enemigos, habilidades
├── items/           comida, recetas, recursos
├── quests/          quests.json
├── interactives/    objetos interactivos de las escenas del prólogo
├── map/map.json     zonas y marcadores del mapa del mundo
├── backgrounds/, avatars/, sounds/, effects/, shaders/
└── locale/          nombres, misiones y traducciones de escenarios en siete idiomas
```

Una partida nueva empieza en `start_scene`, cuyo escenario entrega los objetos iniciales y salta al callejón:

```scenario
scene: start_scene
priority: 0
is_one_run: true
---
action item.add id=food/wolf_meat count=10
action item.add id=resource/branches count=10
switch_scene prolog_alley
end
```

Para cambiar el texto de una escena sin editar este mod, añade un escenario para la misma escena en tu propio mod, o sustituye un archivo enviando otro con el mismo nombre y una dependencia de `cthangover_game`.

## Si algo falla

1. **La tarjeta del juego dice que no está lista.** Usa **Instalar lo que falta** en **Jugar**, o revisa el esquema.
2. **No empieza ningún combate.** El núcleo de combate elegido debe estar incluido en el juego.
3. **El texto de la historia está en ruso.** Es el prólogo (ver Idioma).
