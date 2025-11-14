# Tauri Desktop and Mobile Apps

This document describes the Tauri v2 setup for creating desktop (Windows, Linux) and mobile (Android) applications for Zeitung.

## Overview

Zeitung uses [Tauri v2](https://v2.tauri.app) to create native desktop and mobile applications from the Nuxt frontend. The applications bundle the frontend as a static site and run it in a native webview.

## Prerequisites

### For Desktop Development
- Node.js 22+
- Rust (installed via rustup)
- Platform-specific dependencies:
  - **Windows**: Microsoft Visual Studio C++ Build Tools
  - **Linux**: `libgtk-3-dev`, `libwebkit2gtk-4.1-dev`, `libappindicator3-dev`, `librsvg2-dev`, `patchelf`

### For Android Development
- All desktop prerequisites
- Java Development Kit (JDK) 17+
- Android SDK and NDK (28.2.13676358)
- Android Studio (recommended for debugging)

## Development

### Running the Desktop App

```bash
cd src/frontend
npm run tauri:dev
```

This will start the Nuxt dev server and launch the Tauri desktop app.

### Building for Desktop

#### Windows
```bash
cd src/frontend
npm run tauri:build:windows
```

#### Linux
```bash
cd src/frontend
npm run tauri build -- --target x86_64-unknown-linux-gnu
```

### Running the Android App

First, initialize Android if not already done:
```bash
cd src/frontend
npx tauri android init
```

Then run on an emulator or connected device:
```bash
cd src/frontend
npx tauri android dev
```

### Building for Android

```bash
cd src/frontend
npm run tauri:build:android
```

The APK will be generated in `src/frontend/src-tauri/gen/android/zeitung/app/build/outputs/apk/`.

## CI/CD Integration

The project includes GitHub Actions workflows that:

1. **On Pull Requests and Main Branch Pushes**: Build Tauri apps for Windows, Linux, and Android to verify the builds work
2. **On Release (via release-please)**: Build and publish Tauri apps to GitHub Releases

The workflows automatically:
- Install required dependencies
- Build the frontend in SSG mode
- Compile Rust code for each platform
- Package the applications
- Upload artifacts (PR builds) or release assets (releases)

## Architecture Notes

### Static Site Generation (SSG)

Tauri requires a static frontend. The Nuxt configuration automatically switches to SSG mode when `TAURI_BUILD=true` is set. The generated files are placed in `.output/public/` which Tauri bundles into the native app.

### API Communication

The Tauri apps can communicate with:
- A remote backend API (configured via `API_BASE_URL` environment variable)
- A local backend (for local-first scenarios, see below)

### Multi-Backend Sync

The UI supports multiple backend configurations, allowing users to:
- Switch between different backend servers
- Sync data across multiple backends
- Work with local-first storage

Refer to issue #6 for more details on the multi-backend sync implementation.

## C# Integration (Future Enhancement)

The issue mentions investigating C# integration for local-first feed ingestion. While Tauri's core is Rust-based, potential approaches include:

1. **Rust-C# Interop**: Use FFI (Foreign Function Interface) to call C# libraries from Rust
2. **Separate Process**: Run a C# process alongside the Tauri app and communicate via IPC
3. **gRPC/HTTP**: Have the C# component expose a local API that the Tauri app consumes

This is marked as a future enhancement and requires further investigation to determine the best approach.

## Configuration Files

- `src/frontend/src-tauri/tauri.conf.json`: Main Tauri configuration
- `src/frontend/src-tauri/Cargo.toml`: Rust dependencies
- `src/frontend/nuxt.config.ts`: Frontend configuration with Tauri-specific settings

## Troubleshooting

### Build Failures

1. **Missing dependencies**: Ensure all platform-specific dependencies are installed
2. **OpenAPI schema missing**: The frontend requires `openapi.json`. Generate it from the backend or use a minimal schema for development
3. **Rust compilation errors**: Update Rust toolchain with `rustup update`

### Android Specific Issues

1. **NDK not found**: Install the correct NDK version: `sdkmanager "ndk;28.2.13676358"`
2. **Java version mismatch**: Ensure JDK 17 is installed and set as default
3. **Emulator issues**: Try running on a physical device connected via ADB

## Resources

- [Tauri v2 Documentation](https://v2.tauri.app)
- [Tauri Android Guide](https://v2.tauri.app/develop/mobile/)
- [Nuxt Static Site Generation](https://nuxt.com/docs/getting-started/deployment#static-hosting)
