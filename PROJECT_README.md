# Vector Traffic 3D

Vector Traffic 3D is an original, commercial-quality stylized 3D mobile traffic-routing puzzle game designed for Unity 6 (URP) and Android.

## Core Design Formula:
**VECTOR + ROUTE + GATE**

The player solves miniature traffic networks by reasoning through:
- Multi-cell discrete vehicle footprints (Hatchback = 2, Sedan/SUV/Taxi/Pickup = 3, Van/Bus = 4, Truck = 5)
- Directional vectors (Up, Right, Down, Left)
- Dynamic route graphs and vector-filtering gates
- Multi-exit destination routing (Color + Shape/Symbol)
- Dependency unlocking and congestion unravelling

## Strict Logical Separation
Game logic is completely decoupled from presentation. The deterministic logical simulation runs independently of the 3D renderer, guaranteeing 100% reproducible solver evaluation, generation, and undo state.
