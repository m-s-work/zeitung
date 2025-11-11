# C# Integration for Local-First Feed Ingestion

This document explores potential approaches for integrating C# components into the Tauri-based Zeitung application for local-first feed ingestion on desktop and mobile platforms.

## Context

The issue mentions the possibility of having a C# component running for local-first feed ingestion. Since Tauri is Rust-based, there are several architectural approaches to consider.

## Architectural Approaches

### 1. Rust-C# FFI (Foreign Function Interface)

**How it works:**
- Use P/Invoke or similar mechanisms to call C# functions from Rust
- Compile C# code into native libraries (.dll, .so, .dylib)
- Use Rust FFI to bridge the gap

**Pros:**
- Direct integration within the Tauri process
- Lower overhead than separate processes

**Cons:**
- Complex setup and cross-platform challenges
- Requires native C# compilation (AOT with NativeAOT)
- May not work well on mobile platforms

**Feasibility:** Medium to Low for mobile, Medium for desktop

### 2. Separate Process Communication

**How it works:**
- Run the C# backend as a separate process
- Communicate via IPC (Inter-Process Communication)
- Use named pipes, domain sockets, or stdio

**Pros:**
- Clear separation of concerns
- Can reuse existing C# backend code
- Process isolation improves stability

**Cons:**
- Additional process management complexity
- Not ideal for mobile (battery, resources)
- Requires packaging the .NET runtime

**Feasibility:** High for desktop, Low for mobile

### 3. Local HTTP/gRPC Service

**How it works:**
- Run C# as a local server (localhost:port)
- Tauri app communicates via HTTP/gRPC
- Similar to the existing backend architecture but local

**Pros:**
- Well-established patterns
- Reuse existing backend code
- Easy to test and debug

**Cons:**
- Port management complexity
- Higher overhead than direct FFI
- Security considerations (localhost binding)

**Feasibility:** High for desktop, Medium for mobile

### 4. Rust Rewrite (Recommended for Mobile)

**How it works:**
- Reimplement feed ingestion logic in Rust
- Use existing Rust crates for RSS parsing, HTTP, etc.
- Native Tauri integration

**Pros:**
- Best performance and battery life
- Native mobile support
- Single language stack for frontend
- Smaller app size

**Cons:**
- Requires rewriting C# logic
- Learning curve for Rust

**Feasibility:** High for both desktop and mobile

### 5. WebAssembly Bridge (Experimental)

**How it works:**
- Compile C# to WebAssembly
- Load and execute in the Tauri webview
- Communicate via JS interop

**Pros:**
- Works on all platforms
- No separate process needed

**Cons:**
- Experimental and limited .NET WASM support
- Performance concerns
- Large bundle sizes

**Feasibility:** Low to Medium (experimental)

## Recommended Approach

### For Desktop (Windows, Linux)
**Local HTTP Service** (Approach #3)

- Run the existing .NET backend as a local service
- Configure it to run on `localhost` with a random available port
- Tauri app discovers the port and connects via HTTP
- Benefits from existing C# codebase
- Can be bundled with the Tauri app

Implementation:
```rust
// In Tauri setup
fn start_local_backend() -> Result<u16, Box<dyn std::error::Error>> {
    // Find available port
    // Launch .NET process
    // Return port number
}
```

### For Mobile (Android)
**Rust Rewrite** (Approach #4)

- Implement feed ingestion in Rust using Tauri plugins
- Use crates like:
  - `feed-rs` for RSS/Atom parsing
  - `reqwest` for HTTP requests
  - `sqlx` or `rusqlite` for local storage
- Native performance and battery efficiency
- Proper mobile lifecycle management

Example structure:
```rust
#[tauri::command]
async fn ingest_feed(url: String) -> Result<Feed, String> {
    // Fetch and parse RSS feed
    // Store in local SQLite database
}
```

## SQLite for Local Storage

Regardless of the approach, SQLite is recommended for local storage:
- Native support in both Rust and C#
- Works well on all platforms
- Battle-tested for mobile apps
- Can sync with remote backend

## Multi-Backend Sync

The UI already supports multiple backend configurations (see issue #6). For local-first:

1. Local backend (C# or Rust) handles immediate feed ingestion
2. Background sync process pushes to remote backend when online
3. Conflict resolution using timestamps or CRDTs

## Next Steps

1. **Prototype** a Rust-based feed ingestion module as a Tauri plugin
2. **Evaluate** performance and developer experience
3. **Consider** C# local service for desktop if:
   - Existing C# code is complex
   - Team expertise is primarily C#
   - Desktop-only deployment is acceptable
4. **Implement** sync protocol between local and remote backends

## Resources

- [Tauri Plugins](https://v2.tauri.app/develop/plugins/)
- [Rust FFI](https://doc.rust-lang.org/nomicon/ffi.html)
- [NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [feed-rs](https://crates.io/crates/feed-rs)
- [Tauri Mobile](https://v2.tauri.app/develop/mobile/)
