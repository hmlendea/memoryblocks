# Security

This document describes the security posture, threat model, and mitigations.

## Threat Model

### Assets
- User's local files (none accessed by application)
- System resources (CPU, memory, display)
- Game state (in-memory only, no persistence)
- Embedded resources (piece images, UI definition)

### Trust Boundaries
```
┌─────────────────────────────────────────────────────────┐
│                    User Session                          │
│  ┌──────────────────┐    ┌──────────────────────────┐   │
│  │  MemoryBlocks    │    │      GTK# / Mono         │   │
│  │  (Managed Code)  │───►│  (Native Libraries)      │   │
│  └──────────────────┘    └──────────────────────────┘   │
│           │                        │                     │
│           ▼                        ▼                     │
│  ┌──────────────────────────────────────────────────┐   │
│  │              X11 / Wayland Display Server         │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

### Attack Surface
| Vector | Exposure | Risk |
|--------|----------|------|
| Network | None | N/A |
| File I/O | None (read-only embedded resources) | Low |
| User Input | Mouse clicks on drawing area | Medium |
| Command Line | Args ignored | None |
| Environment Variables | Not read | None |
| Inter-process | None | None |
| Clipboard | Not used | None |
| D-Bus | Not used | None |

## Security Properties

### Confidentiality
- **No sensitive data processed** — no passwords, tokens, PII, encryption keys
- **No data exfiltration possible** — no network, no file write, no clipboard
- **Memory only** — game state in RAM, cleared on exit

### Integrity
- **No external input validation** — click coordinates used directly as array indices
- **No code integrity verification** — no strong naming, no signature verification
- **Embedded resources trusted** — piece images loaded without validation

### Availability
- **Single-threaded** — GTK main loop, no concurrency issues
- **No resource limits** — unbounded Bitmap allocations in render loop
- **Crash on error** — any exception terminates application

## Vulnerabilities

### 1. Array Index Out of Bounds (CWE-129)
**Location:** `MainWindow.OnDaTableButtonPressEvent`
```csharp
int x = (int)args.Event.X / tileSize;
int y = (int)args.Event.Y / tileSize;
game.FlipPiece(x, y);  // No bounds check
```
**Impact:** Application crash (DoS)
**Exploitability:** Click outside drawing area
**Mitigation:** Add bounds check (see error-handling.md)

### 2. Uncontrolled Resource Consumption (CWE-400)
**Location:** `GameEngine.DrawTable` — new `Bitmap` per flipped tile per frame
```csharp
using (Bitmap bmp = ...) { gfx.DrawImage(bmp, rec); }
```
**Impact:** Memory exhaustion under rapid redraw
**Exploitability:** Trigger rapid redraws (resize window, expose events)
**Mitigation:** Cache Bitmaps, pool allocations

### 3. Missing Resource Validation (CWE-1104)
**Location:** `GameEngine.DrawTable`
```csharp
Gdk.Pixbuf.LoadFromResource("...piece" + piece.Value + ".png")
```
**Impact:** Crash if `piece.Value` ≥ 37 (only 37 images exist)
**Exploitability:** Requires memory corruption to set invalid `piece.Value`
**Mitigation:** Validate at startup, clamp values

### 4. Use of Outdated Components (CWE-1104)
- GTK# 2.12 (GTK 2.x) — EOL, no security updates
- .NET Framework 4.5 — EOL, no security updates
- Mono 4.5+ — legacy runtime
- `System.Drawing` via libgdiplus — native code exposure

### 5. No Input Sanitisation (CWE-20)
- Mouse coordinates used directly
- No validation of event data
- Right-click (button 3) has empty handler — potential future bug

### 6. Information Exposure Through Error Messages (CWE-209)
- Unhandled exceptions show stack traces to user (if debugger attached)
- No custom error handling — raw .NET exceptions

## Security Controls (Current)

| Control | Status | Notes |
|---------|--------|-------|
| Authentication | ❌ N/A | Single-user local game |
| Authorisation | ❌ N/A | No permissions model |
| Encryption at Rest | ❌ N/A | No persistent data |
| Encryption in Transit | ❌ N/A | No network |
| Input Validation | ❌ Missing | Critical gap |
| Output Encoding | ❌ N/A | No HTML/web output |
| Logging | ❌ Missing | See logging.md |
| Error Handling | ❌ Missing | See error-handling.md |
| Dependency Scanning | ❌ Missing | No SBOM, no vulnerability scan |
| Code Signing | ❌ Missing | Unsigned assemblies |
| ASLR/DEP | ✅ OS-level | Mono/.NET runtime |
| Stack Protection | ✅ Compiler | Default in Mono |

## Secure Development Practices (Not Currently Followed)

| Practice | Status |
|----------|--------|
| Threat modelling | ❌ Not done |
| Secure code review | ❌ Not done |
| Static analysis (SAST) | ❌ Not configured |
| Dependency scanning (SCA) | ❌ Not configured |
| Penetration testing | ❌ Not done |
| Security training | ❌ Not tracked |
| Incident response | ❌ Not defined |

## Recommended Security Improvements

### Immediate (Defensive)
1. **Bounds checking** on all array accesses from user input
2. **Null checks** on all GDK drawables before use
3. **Resource validation** at startup — verify all piece images exist
4. **Clamp piece values** to valid range (0–36) in `GameTable.GenerateTable`

### Short-term (Hardening)
1. **Bitmap caching** — load piece images once, reuse `Bitmap` objects
2. **Global exception handler** — catch unhandled exceptions, log, show friendly dialog
3. **Remove empty right-click handler** — either implement or remove
4. **Validate grid size** — ensure even, within 4–10 range

### Medium-term (Modernisation)
1. **Migrate to GTK 3/4** — supported, maintained toolkit
2. **Migrate to .NET 8+** — supported runtime, security updates
3. **Replace System.Drawing** — use Cairo/SkiaSharp for cross-platform rendering
4. **Add dependency scanning** — integrate in CI/CD
5. **Sign assemblies** — strong naming, Authenticode for Windows

### Long-term (Architecture)
1. **Sandboxing** — Flatpak/snap/AppImage with minimal permissions
2. **Capability-based security** — no file system, no network, display only
3. **Formal verification** — critical game logic (match detection)

## Compliance

| Standard | Applicability | Status |
|----------|---------------|--------|
| GDPR | No personal data | N/A |
| SOC 2 | Not a service | N/A |
| ISO 27001 | Not applicable | N/A |
| PCI DSS | No payments | N/A |
| OWASP Top 10 | Desktop app | Partial (see vulnerabilities) |

## Security Testing

No security testing currently performed. Recommended:

| Test | Tool/Method |
|------|-------------|
| Fuzzing click coordinates | Custom test harness |
| Memory pressure testing | `valgrind`, `dotnet-counters` |
| Dependency vulnerability scan | `dotnet list package --vulnerable`, `trivy` |
| Static analysis | `SonarQube`, `CodeQL` |
| Binary analysis | `checksec`, `hardening-check` |

## Incident Response

No incident response plan. If vulnerability discovered:
1. Assess impact (crash vs code execution)
2. Patch source (bounds check, validation)
3. Rebuild and redistribute
4. No auto-update mechanism — users must manually update

## Security Contacts

No dedicated security contact. Report issues via:
- GitHub Issues (public)
- Repository maintainer (if known)

## Summary

**Overall Risk: LOW** — Local single-user game with no network, no persistent data, no sensitive operations. Primary risks are denial-of-service via crashes and use of EOL components. No remote code execution vector identified.