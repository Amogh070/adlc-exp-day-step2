import { expect } from 'vitest'

// @testing-library/jest-dom expects a global `expect` (Jest-like).
// We set it before loading jest-dom.
;(globalThis as any).expect = expect

await import('@testing-library/jest-dom')
