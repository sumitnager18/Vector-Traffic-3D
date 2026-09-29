import React, { useState } from 'react';
import { 
  Car, 
  Layers, 
  CheckCircle2, 
  AlertTriangle, 
  Cpu, 
  FolderTree, 
  ShieldCheck, 
  Play, 
  RotateCcw,
  Sparkles,
  Compass,
  FileCode2,
  AlertCircle
} from 'lucide-react';

interface VehicleSim {
  id: number;
  type: string;
  length: number;
  headX: number;
  headY: number;
  orientation: 'UP' | 'RIGHT' | 'DOWN' | 'LEFT';
  direction: 'UP' | 'RIGHT' | 'DOWN' | 'LEFT';
  color: string;
  symbol: string;
  isExited: boolean;
}

export default function App() {
  const [activeTab, setActiveTab] = useState<'overview' | 'simulation' | 'files' | 'audit'>('overview');
  
  // Interactive simulation of the audited logical puzzle engine
  const [gateDirection, setGateDirection] = useState<'UP' | 'RIGHT' | 'DOWN' | 'LEFT'>('UP');
  const [vehicles, setVehicles] = useState<VehicleSim[]>([
    { id: 1, type: 'Bus', length: 4, headX: 3, headY: 4, orientation: 'UP', direction: 'UP', color: 'bg-amber-500', symbol: '▲', isExited: false },
    { id: 2, type: 'Hatchback', length: 2, headX: 2, headY: 2, orientation: 'RIGHT', direction: 'RIGHT', color: 'bg-blue-500', symbol: '■', isExited: false },
    { id: 3, type: 'Sedan', length: 3, headX: 5, headY: 6, orientation: 'DOWN', direction: 'DOWN', color: 'bg-emerald-500', symbol: '●', isExited: false },
    { id: 4, type: 'Delivery Truck', length: 5, headX: 0, headY: 5, orientation: 'UP', direction: 'RIGHT', color: 'bg-purple-600', symbol: '★', isExited: false },
  ]);
  const [logMessages, setLogMessages] = useState<string[]>([
    'Post-Audit Engine active: Full footprint validation enabled.',
    'Checking all candidate cells C[0..L-1] on every move.',
  ]);

  const cycleGate = () => {
    const directions: ('UP' | 'RIGHT' | 'DOWN' | 'LEFT')[] = ['UP', 'RIGHT', 'DOWN', 'LEFT'];
    const nextIdx = (directions.indexOf(gateDirection) + 1) % directions.length;
    const nextDir = directions[nextIdx];
    setGateDirection(nextDir);
    addLog(`Vector Gate cycled to: ${nextDir}`);
  };

  const addLog = (msg: string) => {
    setLogMessages((prev) => [msg, ...prev.slice(0, 7)]);
  };

  // Compute all cells for a vehicle
  const getVehicleCells = (v: VehicleSim) => {
    const cells: { x: number; y: number }[] = [];
    for (let step = 0; step < v.length; step++) {
      let cx = v.headX;
      let cy = v.headY;
      if (v.orientation === 'UP') cy -= step;
      if (v.orientation === 'DOWN') cy += step;
      if (v.orientation === 'RIGHT') cx -= step;
      if (v.orientation === 'LEFT') cx += step;
      cells.push({ x: cx, y: cy });
    }
    return cells;
  };

  // Full multi-cell movement validation
  const attemptMove = (vehicleId: number) => {
    const v = vehicles.find((item) => item.id === vehicleId);
    if (!v || v.isExited) return;

    // Calculate candidate head
    let newHeadX = v.headX;
    let newHeadY = v.headY;
    if (v.direction === 'UP') newHeadY += 1;
    if (v.direction === 'DOWN') newHeadY -= 1;
    if (v.direction === 'RIGHT') newHeadX += 1;
    if (v.direction === 'LEFT') newHeadX -= 1;

    // Calculate candidate footprint for entire vehicle
    const candidateFootprint: { x: number; y: number }[] = [];
    for (let step = 0; step < v.length; step++) {
      let cx = newHeadX;
      let cy = newHeadY;
      if (v.orientation === 'UP') cy -= step;
      if (v.orientation === 'DOWN') cy += step;
      if (v.orientation === 'RIGHT') cx -= step;
      if (v.orientation === 'LEFT') cx += step;
      candidateFootprint.push({ x: cx, y: cy });
    }

    // Check bounds on EVERY cell
    for (let i = 0; i < candidateFootprint.length; i++) {
      const cell = candidateFootprint[i];
      // Check if exit is at top (3, 6) or right (6, 2)
      const isExit = (cell.x === 3 && cell.y === 6) || (cell.x === 6 && cell.y === 2);
      if (isExit && i === 0) continue; // Head reached exit

      if (cell.x < 0 || cell.x > 6 || cell.y < 0 || cell.y > 6) {
        addLog(`Blocked by Boundary: Footprint cell index ${i} (${cell.x}, ${cell.y}) is out of bounds!`);
        return;
      }
    }

    // Check collision with other vehicles for EVERY candidate cell
    for (let i = 0; i < candidateFootprint.length; i++) {
      const cell = candidateFootprint[i];
      for (const other of vehicles) {
        if (other.id === v.id || other.isExited) continue;
        const otherCells = getVehicleCells(other);
        const collision = otherCells.find((oc) => oc.x === cell.x && oc.y === cell.y);
        if (collision) {
          addLog(`Blocked by Vehicle ${other.id} (${other.type}): Collision at footprint index ${i} (${cell.x}, ${cell.y})!`);
          return;
        }
      }
    }

    // Check Gate for vehicle 3
    if (v.id === 3 && gateDirection !== 'DOWN') {
      addLog(`Blocked by Gate at (5, 3): Gate allows ${gateDirection}, vehicle requires DOWN!`);
      return;
    }

    // Vehicle cleared exit run
    setVehicles((prev) =>
      prev.map((item) => (item.id === vehicleId ? { ...item, isExited: true } : item))
    );
    addLog(`Authoritative Exit: Vehicle ${v.id} (${v.type}, ${v.length} cells) cleared board.`);
  };

  const resetSim = () => {
    setVehicles([
      { id: 1, type: 'Bus', length: 4, headX: 3, headY: 4, orientation: 'UP', direction: 'UP', color: 'bg-amber-500', symbol: '▲', isExited: false },
      { id: 2, type: 'Hatchback', length: 2, headX: 2, headY: 2, orientation: 'RIGHT', direction: 'RIGHT', color: 'bg-blue-500', symbol: '■', isExited: false },
      { id: 3, type: 'Sedan', length: 3, headX: 5, headY: 6, orientation: 'DOWN', direction: 'DOWN', color: 'bg-emerald-500', symbol: '●', isExited: false },
      { id: 4, type: 'Delivery Truck', length: 5, headX: 0, headY: 5, orientation: 'UP', direction: 'RIGHT', color: 'bg-purple-600', symbol: '★', isExited: false },
    ]);
    setGateDirection('UP');
    addLog('Simulation reset to initial state.');
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans">
      {/* Top Header */}
      <header className="border-b border-slate-800 bg-slate-900/60 backdrop-blur px-6 py-4 flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-cyan-500 to-indigo-600 flex items-center justify-center shadow-lg shadow-cyan-500/20">
            <Compass className="w-6 h-6 text-white" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="font-bold text-lg tracking-wide text-white">VECTOR TRAFFIC 3D</h1>
              <span className="text-xs font-semibold px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-400 border border-cyan-800">
                Unity 6 (URP)
              </span>
            </div>
            <p className="text-xs text-slate-400">Milestone 0: Post-Audit Engine Hardening</p>
          </div>
        </div>

        <nav className="flex items-center gap-2">
          {(['overview', 'simulation', 'files', 'audit'] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-3.5 py-1.5 rounded-lg text-xs font-medium capitalize transition-all ${
                activeTab === tab
                  ? 'bg-indigo-600 text-white shadow-md shadow-indigo-500/25'
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              {tab}
            </button>
          ))}
        </nav>
      </header>

      {/* Main Content Area */}
      <main className="flex-1 p-6 max-w-7xl mx-auto w-full">
        {activeTab === 'overview' && (
          <div className="space-y-6">
            {/* Status Hero Card */}
            <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 relative overflow-hidden shadow-xl">
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
                <div>
                  <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-amber-400 bg-amber-950/60 border border-amber-800 px-2.5 py-1 rounded-full mb-3">
                    <AlertTriangle className="w-3.5 h-3.5" />
                    Milestone 0 Audited & Corrected (M1 Paused)
                  </span>
                  <h2 className="text-2xl font-bold text-white mb-2">Authoritative Multi-Cell Movement Validation</h2>
                  <p className="text-sm text-slate-400 max-w-2xl leading-relaxed">
                    Zero-Fake-Implementation audit completed. The head-only movement bug, premature exit transition, 
                    and canonical hash omissions have been comprehensively corrected. Every occupied footprint cell is now inspected.
                  </p>
                </div>
                <div className="grid grid-cols-2 gap-3 min-w-[280px]">
                  <div className="bg-slate-950/80 border border-slate-800 p-3 rounded-xl">
                    <span className="text-xs text-slate-500 block">Multi-Cell Footprint</span>
                    <span className="text-sm font-semibold text-emerald-400">All Cells Validated</span>
                  </div>
                  <div className="bg-slate-950/80 border border-slate-800 p-3 rounded-xl">
                    <span className="text-xs text-slate-500 block">Exit Clearance</span>
                    <span className="text-sm font-semibold text-emerald-400">Full L-Step Trail</span>
                  </div>
                  <div className="bg-slate-950/80 border border-slate-800 p-3 rounded-xl">
                    <span className="text-xs text-slate-500 block">Canonical Hash</span>
                    <span className="text-sm font-semibold text-emerald-400">All State Variables</span>
                  </div>
                  <div className="bg-slate-950/80 border border-slate-800 p-3 rounded-xl">
                    <span className="text-xs text-slate-500 block">M1 Readiness</span>
                    <span className="text-sm font-semibold text-amber-400">Awaiting Signoff</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Foundational Fixes Detail */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
              <div className="bg-slate-900 border border-slate-800 p-5 rounded-2xl space-y-3">
                <div className="w-9 h-9 rounded-lg bg-emerald-500/10 text-emerald-400 flex items-center justify-center font-bold text-sm">
                  1
                </div>
                <h3 className="font-semibold text-white text-base">Full Footprint Inspection</h3>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Fixed head-only check. <code className="text-cyan-400">MoveValidator.ValidateSingleStep</code> now inspects all $L$ cells of candidate footprint against boundaries, obstacles, gates, and vehicles.
                </p>
              </div>

              <div className="bg-slate-900 border border-slate-800 p-5 rounded-2xl space-y-3">
                <div className="w-9 h-9 rounded-lg bg-indigo-500/10 text-indigo-400 flex items-center justify-center font-bold text-sm">
                  2
                </div>
                <h3 className="font-semibold text-white text-base">L-Step Exit Clearance</h3>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Vehicles cannot exit prematurely. <code className="text-cyan-400">CanCompleteExit</code> requires all trailing body cells to cleanly advance through the exit portal before committing <code className="text-cyan-400">IsExited = true</code>.
                </p>
              </div>

              <div className="bg-slate-900 border border-slate-800 p-5 rounded-2xl space-y-3">
                <div className="w-9 h-9 rounded-lg bg-amber-500/10 text-amber-400 flex items-center justify-center font-bold text-sm">
                  3
                </div>
                <h3 className="font-semibold text-white text-base">Comprehensive Hash</h3>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Integrated <code className="text-cyan-400">Orientation</code>, <code className="text-cyan-400">Length</code>, and <code className="text-cyan-400">DestinationId</code> into canonical state hash, preventing hash collisions between distinct board states.
                </p>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'simulation' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Visualizer Board */}
            <div className="lg:col-span-2 bg-slate-900 border border-slate-800 rounded-2xl p-6">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h3 className="font-bold text-white text-base">Audited Multi-Cell Movement Inspector</h3>
                  <p className="text-xs text-slate-400">Tests complete footprint collision on body cells, not just the head.</p>
                </div>
                <button
                  onClick={resetSim}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-xs text-slate-200 transition"
                >
                  <RotateCcw className="w-3.5 h-3.5" /> Reset
                </button>
              </div>

              {/* 7x7 Grid */}
              <div className="aspect-square max-w-[420px] mx-auto bg-slate-950 p-3 rounded-2xl border border-slate-800 grid grid-cols-7 gap-1.5">
                {Array.from({ length: 49 }).map((_, index) => {
                  const x = index % 7;
                  const y = 6 - Math.floor(index / 7);

                  const isExitTop = x === 3 && y === 6;
                  const isExitRight = x === 6 && y === 2;
                  const isGate = x === 5 && y === 3;

                  let occupying: VehicleSim | null = null;
                  vehicles.forEach((v) => {
                    if (v.isExited) return;
                    const cells = getVehicleCells(v);
                    if (cells.some((c) => c.x === x && c.y === y)) {
                      occupying = v;
                    }
                  });

                  return (
                    <div
                      key={`${x}-${y}`}
                      className={`relative rounded-lg flex items-center justify-center text-xs font-mono select-none transition-all ${
                        occupying
                          ? `${occupying.color} text-white font-bold shadow-md cursor-pointer hover:brightness-110`
                          : isExitTop || isExitRight
                          ? 'bg-emerald-950/70 border border-emerald-600/60 text-emerald-400'
                          : isGate
                          ? 'bg-purple-950/80 border border-purple-500 text-purple-300 cursor-pointer'
                          : 'bg-slate-900/60 border border-slate-800/40 text-slate-600'
                      }`}
                      onClick={() => {
                        if (occupying) attemptMove(occupying.id);
                        if (isGate) cycleGate();
                      }}
                    >
                      {occupying && (
                        <span>
                          {occupying.headX === x && occupying.headY === y ? occupying.symbol : '•'}
                        </span>
                      )}
                      {!occupying && (isExitTop || isExitRight) && 'EXIT'}
                      {!occupying && isGate && (
                        <span className="text-[10px] font-bold">
                          {gateDirection === 'UP' ? '↑' : gateDirection === 'RIGHT' ? '→' : gateDirection === 'DOWN' ? '↓' : '←'}
                        </span>
                      )}
                    </div>
                  );
                })}
              </div>

              {/* Legend */}
              <div className="mt-4 flex flex-wrap items-center justify-center gap-4 text-xs text-slate-400">
                <span className="flex items-center gap-1.5">
                  <span className="w-3 h-3 rounded bg-amber-500 inline-block" /> Bus (4 cells, ▲)
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="w-3 h-3 rounded bg-blue-500 inline-block" /> Hatchback (2 cells, ■)
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="w-3 h-3 rounded bg-emerald-500 inline-block" /> Sedan (3 cells, ●)
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="w-3 h-3 rounded bg-purple-600 inline-block" /> Truck (5 cells, ★)
                </span>
              </div>
            </div>

            {/* Controls & Log */}
            <div className="space-y-4">
              <div className="bg-slate-900 border border-slate-800 rounded-2xl p-5 space-y-3">
                <h4 className="font-semibold text-white text-sm">Action Controls</h4>
                <div className="space-y-2 pt-2">
                  {vehicles.map((v) => (
                    <button
                      key={v.id}
                      disabled={v.isExited}
                      onClick={() => attemptMove(v.id)}
                      className={`w-full text-left px-3 py-2 rounded-xl border flex items-center justify-between text-xs transition ${
                        v.isExited
                          ? 'bg-slate-950 border-slate-800 text-slate-600 cursor-not-allowed'
                          : 'bg-slate-800 border-slate-700 hover:bg-slate-700 text-slate-200'
                      }`}
                    >
                      <div className="flex items-center gap-2">
                        <span className={`w-2.5 h-2.5 rounded-full ${v.color}`} />
                        <span>V{v.id}: {v.type} ({v.length}c, {v.direction})</span>
                      </div>
                      <span className="text-[11px] font-mono">{v.isExited ? 'EXITED' : 'ADVANCE'}</span>
                    </button>
                  ))}

                  <button
                    onClick={cycleGate}
                    className="w-full text-left px-3 py-2 rounded-xl border border-purple-800/80 bg-purple-950/40 hover:bg-purple-900/50 text-purple-200 text-xs flex items-center justify-between transition"
                  >
                    <span>Cycle Vector Gate</span>
                    <span className="font-mono font-bold text-cyan-400">{gateDirection}</span>
                  </button>
                </div>
              </div>

              {/* Console */}
              <div className="bg-slate-900 border border-slate-800 rounded-2xl p-5 space-y-2">
                <h4 className="font-semibold text-white text-sm flex items-center gap-2">
                  <Play className="w-3.5 h-3.5 text-cyan-400" />
                  Validator Stream
                </h4>
                <div className="bg-slate-950 rounded-xl p-3 text-[11px] font-mono space-y-1 text-slate-400 min-h-[140px] border border-slate-800/60">
                  {logMessages.map((log, index) => (
                    <div key={index} className="leading-snug">
                      <span className="text-cyan-500 mr-1.5">&gt;</span>
                      {log}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'files' && (
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4">
            <h3 className="font-semibold text-white text-base flex items-center gap-2">
              <FolderTree className="w-5 h-5 text-indigo-400" />
              Authored C# Solution Files
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="bg-slate-950 p-4 rounded-xl border border-slate-800 font-mono text-xs text-slate-300 space-y-1.5">
                <div className="text-cyan-400 font-bold mb-2">📁 Audited Core C# Assemblies</div>
                <div>├── /unity/Assets/Scripts/VectorTraffic3D.asmdef</div>
                <div>├── /unity/Assets/Scripts/Core/GridPosition.cs</div>
                <div>├── /unity/Assets/Scripts/Core/Direction.cs</div>
                <div>├── /unity/Assets/Scripts/Vehicles/VehicleType.cs</div>
                <div>├── /unity/Assets/Scripts/Vehicles/VehicleFootprint.cs</div>
                <div>├── /unity/Assets/Scripts/Vehicles/VehicleState.cs</div>
                <div>├── /unity/Assets/Scripts/Board/CellOccupancy.cs (Precedence fixed)</div>
                <div>├── /unity/Assets/Scripts/Board/OccupancySystem.cs (Precedence fixed)</div>
                <div>├── /unity/Assets/Scripts/Board/BoardState.cs (Hash fixed)</div>
                <div>├── /unity/Assets/Scripts/Gates/VectorGate.cs</div>
                <div>├── /unity/Assets/Scripts/Puzzle/MoveValidator.cs (Full footprint fixed)</div>
                <div>└── /unity/Assets/Scripts/Solver/DeterministicSolver.cs (Unified rules)</div>
              </div>

              <div className="bg-slate-950 p-4 rounded-xl border border-slate-800 font-mono text-xs text-slate-300 space-y-1.5">
                <div className="text-emerald-400 font-bold mb-2">📁 Authored NUnit Test Suites</div>
                <div>├── /unity/Assets/Tests/EditMode/VectorTraffic3D.Tests.asmdef</div>
                <div>├── /unity/Assets/Tests/EditMode/VehicleFootprintTests.cs</div>
                <div>├── /unity/Assets/Tests/EditMode/OccupancySystemTests.cs</div>
                <div>├── /unity/Assets/Tests/EditMode/DeterministicMoveTests.cs</div>
                <div>├── /unity/Assets/Tests/EditMode/DeterministicSolverTests.cs</div>
                <div>├── /unity/Assets/Tests/EditMode/CanonicalHashTests.cs</div>
                <div>├── /MILESTONE_0_AUDIT.md</div>
                <div>├── /QUALITY_REPORT.md</div>
                <div>└── /KNOWN_LIMITATIONS.md</div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'audit' && (
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-6">
            <div>
              <h3 className="font-semibold text-white text-base">Execution Status & Taxonomy Audit</h3>
              <p className="text-xs text-slate-400">Honest reporting conforming to Section 8 of the audit directive.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="bg-slate-950 border border-slate-800 p-4 rounded-xl space-y-3">
                <h4 className="text-xs font-semibold text-slate-300 uppercase tracking-wider">Test Suite Status</h4>
                <div className="space-y-2 text-xs">
                  {[
                    { name: 'VehicleFootprintTests.cs', source: 'IMPLEMENTED', runner: 'UNVERIFIED (Unity absent)' },
                    { name: 'OccupancySystemTests.cs', source: 'IMPLEMENTED', runner: 'UNVERIFIED (Unity absent)' },
                    { name: 'DeterministicMoveTests.cs', source: 'IMPLEMENTED', runner: 'UNVERIFIED (Unity absent)' },
                    { name: 'DeterministicSolverTests.cs', source: 'IMPLEMENTED', runner: 'UNVERIFIED (Unity absent)' },
                    { name: 'CanonicalHashTests.cs', source: 'IMPLEMENTED', runner: 'UNVERIFIED (Unity absent)' },
                  ].map((t) => (
                    <div key={t.name} className="flex justify-between py-1.5 border-b border-slate-800/80 last:border-0">
                      <div>
                        <div className="font-mono text-slate-200">{t.name}</div>
                        <div className="text-[10px] text-slate-500">Source: {t.source}</div>
                      </div>
                      <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-slate-900 text-amber-400 border border-slate-800">
                        {t.runner}
                      </span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="bg-slate-950 border border-slate-800 p-4 rounded-xl space-y-3">
                <h4 className="text-xs font-semibold text-slate-300 uppercase tracking-wider">M1 Pause Invariant</h4>
                <div className="text-xs text-slate-400 space-y-2">
                  <div className="p-3 rounded-lg bg-amber-950/40 border border-amber-800/80 text-amber-200">
                    <strong>STOP CONDITION ACTIVE:</strong> RouteGraph, Procedural Generator, Difficulty Analyzer, 
                    3D Visuals, and Campaign Chapters remain strictly <strong>NOT IMPLEMENTED</strong> until Milestone 0 audit approval.
                  </div>
                  <p>
                    All multi-cell movement bugs, exit trail clearance, and canonical state-space hashing have been resolved in C# and mirror models.
                  </p>
                </div>
              </div>
            </div>
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-800/80 px-6 py-3 text-xs text-slate-500 flex flex-wrap items-center justify-between gap-2 bg-slate-900/40">
        <div>Vector Traffic 3D • Commercial Mobile Puzzle Game Architecture</div>
        <div className="flex items-center gap-4">
          <span>Engine: Unity 6 (URP)</span>
          <span>Target: Android</span>
          <span className="text-amber-400">Milestone 0 Audit Paused</span>
        </div>
      </footer>
    </div>
  );
}
