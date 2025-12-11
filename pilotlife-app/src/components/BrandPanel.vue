<template>
  <div class="brand-panel">
    <div class="logo-container">
      <!-- Animated Wordmark Logo - Plane flies across -->
      <div class="wordmark-logo animated-logo">
        <svg viewBox="0 0 280 70" fill="none" xmlns="http://www.w3.org/2000/svg">
          <!-- "PilotLife" Text -->
          <text
            x="10"
            y="42"
            class="logo-wordmark"
            fill="url(#text-grad)"
            font-family="Outfit, sans-serif"
            font-size="42"
            font-weight="800"
          >PilotLife</text>

          <!-- Contrail underline - draws as plane flies -->
          <path
            d="M 10 55 Q 80 55 140 54 Q 200 53 240 52"
            stroke="url(#contrail-grad)"
            stroke-width="4"
            stroke-linecap="round"
            fill="none"
            class="contrail-line-animated"
          />

          <!-- Secondary thinner contrail -->
          <path
            d="M 30 58 Q 90 57 150 56 Q 200 55 237 54"
            stroke="url(#contrail-grad-light)"
            stroke-width="2"
            stroke-linecap="round"
            fill="none"
            class="contrail-line-thin-animated"
          />

          <!-- Airplane flies across -->
          <g class="airplane-flying">
            <!-- Fuselage -->
            <ellipse cx="8" cy="4" rx="12" ry="3" fill="url(#plane-grad)"/>

            <!-- Nose -->
            <path d="M 18 2 Q 24 4 18 6 Z" fill="url(#plane-grad)"/>

            <!-- Top wing -->
            <path d="M 6 2 L 2 -4 L 6 -4 L 12 1 Z" fill="url(#plane-grad)"/>

            <!-- Bottom wing -->
            <path d="M 6 6 L 2 12 L 6 12 L 12 7 Z" fill="url(#plane-grad)"/>

            <!-- Tail fin -->
            <path d="M -2 4 L -4 -1 L 0 -1 L 2 3 Z" fill="url(#plane-grad)"/>

            <!-- Horizontal stabilizer -->
            <path d="M -2 3 L -4 1 L 0 2 Z" fill="url(#plane-grad)"/>
            <path d="M -2 5 L -4 7 L 0 6 Z" fill="url(#plane-grad)"/>

            <!-- Cockpit highlight -->
            <ellipse cx="16" cy="4" rx="2" ry="1" fill="white" opacity="0.6"/>
          </g>

          <defs>
            <!-- Text gradient -->
            <linearGradient id="text-grad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stop-color="#0ea5e9"/>
              <stop offset="100%" stop-color="#8b5cf6"/>
            </linearGradient>
            <!-- Contrail gradient - fades in from left -->
            <linearGradient id="contrail-grad" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
              <stop offset="30%" stop-color="#0ea5e9" stop-opacity="0.4"/>
              <stop offset="70%" stop-color="#06b6d4" stop-opacity="0.7"/>
              <stop offset="100%" stop-color="#8b5cf6" stop-opacity="1"/>
            </linearGradient>
            <!-- Lighter contrail gradient -->
            <linearGradient id="contrail-grad-light" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0"/>
              <stop offset="40%" stop-color="#06b6d4" stop-opacity="0.2"/>
              <stop offset="100%" stop-color="#8b5cf6" stop-opacity="0.5"/>
            </linearGradient>
            <!-- Plane gradient -->
            <linearGradient id="plane-grad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stop-color="#0ea5e9"/>
              <stop offset="100%" stop-color="#8b5cf6"/>
            </linearGradient>
          </defs>
        </svg>
      </div>
      <div class="logo-tagline">Your Virtual Career Awaits</div>
    </div>

    <ul class="features-list">
      <li>
        <div class="feature-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2" stroke-linecap="round">
            <path d="M12 2L2 7L12 12L22 7L12 2Z"/>
            <path d="M2 17L12 22L22 17"/>
            <path d="M2 12L12 17L22 12"/>
          </svg>
        </div>
        <div class="feature-text">
          <h4>Dynamic Economy</h4>
          <p>Real-time market simulation with live cargo pricing and passenger demand curves</p>
        </div>
      </li>
      <li>
        <div class="feature-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2" stroke-linecap="round">
            <path d="M22 12H18L15 21L9 3L6 12H2"/>
          </svg>
        </div>
        <div class="feature-text">
          <h4>Smart Flight Tracking</h4>
          <p>Seamless MSFS integration with detailed performance analytics and insights</p>
        </div>
      </li>
      <li>
        <div class="feature-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2" stroke-linecap="round">
            <path d="M12 20V10"/>
            <path d="M18 20V4"/>
            <path d="M6 20V16"/>
          </svg>
        </div>
        <div class="feature-text">
          <h4>Career Progression</h4>
          <p>Build your airline empire from regional routes to global operations</p>
        </div>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.brand-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 60px;
  position: relative;
  z-index: 2;
}

.logo-container {
  text-align: center;
  margin-bottom: 60px;
}

/* Wordmark Logo Styles */
.wordmark-logo {
  width: 320px;
  margin: 0 auto 16px;
}

.wordmark-logo svg {
  width: 100%;
  height: auto;
  filter: drop-shadow(0 4px 12px rgba(14, 165, 233, 0.15));
}

/* Logo Animations */

/* Animated Logo - Plane flies across and contrail draws behind */
.animated-logo :deep(.airplane-flying) {
  animation: plane-fly 2.5s linear forwards;
  transform-origin: center;
}

.animated-logo :deep(.contrail-line-animated) {
  stroke-dasharray: 250;
  stroke-dashoffset: 250;
  animation: draw-contrail 2.5s linear forwards;
}

.animated-logo :deep(.contrail-line-thin-animated) {
  stroke-dasharray: 230;
  stroke-dashoffset: 230;
  animation: draw-contrail-thin 2.5s linear 0.1s forwards;
}

/* Plane flies from left side to final position - starts ahead of contrail */
@keyframes plane-fly {
  0% {
    transform: translate(-5px, 47px) rotate(-2deg);
    opacity: 0;
  }
  2% {
    opacity: 1;
  }
  100% {
    transform: translate(245px, 40px) rotate(-3deg);
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
    transform: translate(245px, 40px) rotate(-3deg);
  }
  50% {
    transform: translate(247px, 38px) rotate(-1deg);
  }
}

.logo-tagline {
  font-size: 14px;
  letter-spacing: 4px;
  color: var(--text-secondary);
  text-transform: uppercase;
  font-weight: 500;
  text-align: center;
}

.features-list {
  list-style: none;
  max-width: 380px;
  padding: 0;
}

.features-list li {
  display: flex;
  align-items: flex-start;
  gap: 18px;
  margin-bottom: 28px;
  opacity: 0;
  animation: fadeSlideIn 0.6s ease forwards;
}

.features-list li:nth-child(1) { animation-delay: 0.3s; }
.features-list li:nth-child(2) { animation-delay: 0.45s; }
.features-list li:nth-child(3) { animation-delay: 0.6s; }

@keyframes fadeSlideIn {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.feature-icon {
  width: 52px;
  height: 52px;
  background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.25);
}

.feature-icon svg {
  width: 24px;
  height: 24px;
  stroke: white;
}

.feature-text h4 {
  font-size: 17px;
  font-weight: 600;
  margin-bottom: 6px;
  color: var(--text-primary);
}

.feature-text p {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
}
</style>
