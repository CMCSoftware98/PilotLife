<template>
  <div class="bg-decoration">
    <div class="bg-blob blob-1"></div>
    <div class="bg-blob blob-2"></div>
    <div class="bg-blob blob-3"></div>
  </div>
  <div class="plane-decoration">
    <svg viewBox="0 0 24 24" fill="currentColor">
      <path d="M21 16v-2l-8-5V3.5c0-.83-.67-1.5-1.5-1.5S10 2.67 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L13 19v-5.5l8 2.5z"/>
    </svg>
  </div>
</template>

<style scoped>
.bg-decoration {
  position: fixed;
  inset: 0;
  z-index: 0;
  overflow: hidden;
  pointer-events: none;
}

.bg-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.6;
  animation: float 20s ease-in-out infinite;
}

.bg-blob.blob-1 {
  width: 500px;
  height: 500px;
  background: linear-gradient(135deg, #bfdbfe, #ddd6fe);
  top: -10%;
  left: -5%;
  animation-delay: 0s;
}

.bg-blob.blob-2 {
  width: 400px;
  height: 400px;
  background: linear-gradient(135deg, #a5f3fc, #99f6e4);
  bottom: -10%;
  right: 30%;
  animation-delay: -7s;
}

.bg-blob.blob-3 {
  width: 300px;
  height: 300px;
  background: linear-gradient(135deg, #fecaca, #fed7aa);
  top: 40%;
  right: -5%;
  animation-delay: -14s;
}

@keyframes float {
  0%, 100% { transform: translate(0, 0) scale(1); }
  25% { transform: translate(30px, -30px) scale(1.05); }
  50% { transform: translate(-20px, 20px) scale(0.95); }
  75% { transform: translate(20px, 30px) scale(1.02); }
}

.plane-decoration {
  position: fixed;
  bottom: 40px;
  left: 40px;
  opacity: 0.08;
  z-index: 1;
}

.plane-decoration svg {
  width: 200px;
  height: 200px;
  fill: var(--accent-primary);
}
</style>
