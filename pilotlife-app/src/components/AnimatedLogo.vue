<template>
  <div class="wordmark-logo animated-logo">
    <svg viewBox="0 0 280 70" fill="none" xmlns="http://www.w3.org/2000/svg">
      <!-- "PilotLife" Text -->
      <text
        x="10"
        y="42"
        class="logo-wordmark"
        fill="url(#text-grad-anim)"
        font-family="Outfit, sans-serif"
        font-size="42"
        font-weight="800"
      >PilotLife</text>

      <!-- Contrail underline - draws as plane flies -->
      <path
        d="M 10 55 Q 80 55 140 54 Q 200 53 248 52"
        stroke="url(#contrail-grad-anim)"
        stroke-width="4"
        stroke-linecap="round"
        fill="none"
        class="contrail-line-animated"
      />

      <!-- Secondary thinner contrail -->
      <path
        d="M 30 58 Q 90 57 150 56 Q 210 55 245 54"
        stroke="url(#contrail-grad-light-anim)"
        stroke-width="2"
        stroke-linecap="round"
        fill="none"
        class="contrail-line-thin-animated"
      />

      <!-- Airplane flies across -->
      <g class="airplane-flying">
        <!-- Fuselage -->
        <ellipse cx="8" cy="4" rx="12" ry="3" fill="url(#plane-grad-anim)"/>

        <!-- Nose -->
        <path d="M 18 2 Q 24 4 18 6 Z" fill="url(#plane-grad-anim)"/>

        <!-- Top wing -->
        <path d="M 6 2 L 2 -4 L 6 -4 L 12 1 Z" fill="url(#plane-grad-anim)"/>

        <!-- Bottom wing -->
        <path d="M 6 6 L 2 12 L 6 12 L 12 7 Z" fill="url(#plane-grad-anim)"/>

        <!-- Tail fin -->
        <path d="M -2 4 L -4 -1 L 0 -1 L 2 3 Z" fill="url(#plane-grad-anim)"/>

        <!-- Horizontal stabilizer -->
        <path d="M -2 3 L -4 1 L 0 2 Z" fill="url(#plane-grad-anim)"/>
        <path d="M -2 5 L -4 7 L 0 6 Z" fill="url(#plane-grad-anim)"/>

        <!-- Cockpit highlight -->
        <ellipse cx="16" cy="4" rx="2" ry="1" fill="white" opacity="0.6"/>
      </g>

      <defs>
        <!-- Text gradient -->
        <linearGradient id="text-grad-anim" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#0ea5e9"/>
          <stop offset="100%" stop-color="#8b5cf6"/>
        </linearGradient>
        <!-- Contrail gradient - fades in from left -->
        <linearGradient id="contrail-grad-anim" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
          <stop offset="30%" stop-color="#0ea5e9" stop-opacity="0.4"/>
          <stop offset="70%" stop-color="#06b6d4" stop-opacity="0.7"/>
          <stop offset="100%" stop-color="#8b5cf6" stop-opacity="1"/>
        </linearGradient>
        <!-- Lighter contrail gradient -->
        <linearGradient id="contrail-grad-light-anim" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
          <stop offset="40%" stop-color="#06b6d4" stop-opacity="0.2"/>
          <stop offset="100%" stop-color="#8b5cf6" stop-opacity="0.5"/>
        </linearGradient>
        <!-- Plane gradient -->
        <linearGradient id="plane-grad-anim" x1="0%" y1="0%" x2="100%" y2="100%">
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

/* Animated Logo - Plane flies across and contrail draws behind */
.animated-logo :deep(.airplane-flying) {
  animation: plane-fly 3s ease-out forwards;
  transform-origin: center;
}

.animated-logo :deep(.contrail-line-animated) {
  stroke-dasharray: 250;
  stroke-dashoffset: 250;
  animation: draw-contrail 3s ease-out forwards;
}

.animated-logo :deep(.contrail-line-thin-animated) {
  stroke-dasharray: 230;
  stroke-dashoffset: 230;
  animation: draw-contrail-thin 3s ease-out 0.15s forwards;
}

/* Plane flies from left side to final position */
@keyframes plane-fly {
  0% {
    transform: translate(-20px, 55px) rotate(-5deg);
    opacity: 0;
  }
  5% {
    opacity: 1;
  }
  100% {
    transform: translate(248px, 48px) rotate(-8deg);
    opacity: 1;
  }
}

/* Main contrail draws behind the plane */
@keyframes draw-contrail {
  0% {
    stroke-dashoffset: 250;
  }
  100% {
    stroke-dashoffset: 0;
  }
}

/* Secondary contrail draws slightly delayed */
@keyframes draw-contrail-thin {
  0% {
    stroke-dashoffset: 230;
  }
  100% {
    stroke-dashoffset: 0;
  }
}

/* After animation completes, add subtle hover effect */
.animated-logo:hover :deep(.airplane-flying) {
  animation: plane-fly 3s ease-out forwards, plane-hover 2s ease-in-out 3s infinite;
}

@keyframes plane-hover {
  0%, 100% {
    transform: translate(248px, 48px) rotate(-8deg);
  }
  50% {
    transform: translate(250px, 46px) rotate(-6deg);
  }
}
</style>
