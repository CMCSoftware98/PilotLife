<template>
  <div class="wordmark-logo">
    <svg viewBox="0 0 280 70" fill="none" xmlns="http://www.w3.org/2000/svg">
      <!-- "PilotLife" Text -->
      <text
        x="10"
        y="42"
        class="logo-wordmark"
        fill="url(#text-grad-static)"
        font-family="Outfit, sans-serif"
        font-size="42"
        font-weight="800"
      >PilotLife</text>

      <!-- Contrail underline - fades from left to right -->
      <path
        d="M 10 55 Q 80 55 140 54 Q 200 53 248 52"
        stroke="url(#contrail-grad-static)"
        stroke-width="4"
        stroke-linecap="round"
        fill="none"
        class="contrail-line"
      />

      <!-- Secondary thinner contrail -->
      <path
        d="M 30 58 Q 90 57 150 56 Q 210 55 245 54"
        stroke="url(#contrail-grad-light-static)"
        stroke-width="2"
        stroke-linecap="round"
        fill="none"
        class="contrail-line-thin"
      />

      <!-- Airplane at the end of the contrail -->
      <g class="airplane" transform="translate(248, 48) rotate(-8)">
        <!-- Fuselage -->
        <ellipse cx="8" cy="4" rx="12" ry="3" fill="url(#plane-grad-static)"/>

        <!-- Nose -->
        <path d="M 18 2 Q 24 4 18 6 Z" fill="url(#plane-grad-static)"/>

        <!-- Top wing -->
        <path d="M 6 2 L 2 -4 L 6 -4 L 12 1 Z" fill="url(#plane-grad-static)"/>

        <!-- Bottom wing -->
        <path d="M 6 6 L 2 12 L 6 12 L 12 7 Z" fill="url(#plane-grad-static)"/>

        <!-- Tail fin -->
        <path d="M -2 4 L -4 -1 L 0 -1 L 2 3 Z" fill="url(#plane-grad-static)"/>

        <!-- Horizontal stabilizer -->
        <path d="M -2 3 L -4 1 L 0 2 Z" fill="url(#plane-grad-static)"/>
        <path d="M -2 5 L -4 7 L 0 6 Z" fill="url(#plane-grad-static)"/>

        <!-- Cockpit highlight -->
        <ellipse cx="16" cy="4" rx="2" ry="1" fill="white" opacity="0.6"/>
      </g>

      <defs>
        <!-- Text gradient -->
        <linearGradient id="text-grad-static" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#0ea5e9"/>
          <stop offset="100%" stop-color="#8b5cf6"/>
        </linearGradient>
        <!-- Contrail gradient - fades in from left -->
        <linearGradient id="contrail-grad-static" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
          <stop offset="30%" stop-color="#0ea5e9" stop-opacity="0.4"/>
          <stop offset="70%" stop-color="#06b6d4" stop-opacity="0.7"/>
          <stop offset="100%" stop-color="#8b5cf6" stop-opacity="1"/>
        </linearGradient>
        <!-- Lighter contrail gradient -->
        <linearGradient id="contrail-grad-light-static" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
          <stop offset="40%" stop-color="#06b6d4" stop-opacity="0.2"/>
          <stop offset="100%" stop-color="#8b5cf6" stop-opacity="0.5"/>
        </linearGradient>
        <!-- Plane gradient -->
        <linearGradient id="plane-grad-static" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#0ea5e9"/>
          <stop offset="100%" stop-color="#8b5cf6"/>
        </linearGradient>
      </defs>
    </svg>
  </div>
</template>

<style scoped>
.wordmark-logo {
  width: 320px;
  margin: 0 auto 16px;
}

.wordmark-logo svg {
  width: 100%;
  height: auto;
  filter: drop-shadow(0 4px 12px rgba(14, 165, 233, 0.15));
}

/* Subtle breathing animations for static logo */
.wordmark-logo :deep(.airplane) {
  animation: plane-float 3s ease-in-out infinite;
}

.wordmark-logo :deep(.contrail-line) {
  animation: contrail-breathe 2.5s ease-in-out infinite;
}

.wordmark-logo :deep(.contrail-line-thin) {
  animation: contrail-breathe 2.5s ease-in-out infinite 0.3s;
}

@keyframes plane-float {
  0%, 100% {
    transform: translate(248px, 48px) rotate(-8deg);
  }
  50% {
    transform: translate(250px, 46px) rotate(-6deg);
  }
}

@keyframes contrail-breathe {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}
</style>
