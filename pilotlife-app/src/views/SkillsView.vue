<template>
  <div class="placeholder-view">
    <div class="placeholder-content">
      <div class="placeholder-icon">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26 12,2"/>
        </svg>
      </div>
      <h1 class="placeholder-title">Skill Points</h1>
      <p class="placeholder-description">
        Upgrade your pilot abilities here. Improve navigation, fuel efficiency, cargo handling, and more.
      </p>
      <div class="coming-soon-badge">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <polyline points="12,6 12,12 16,14"/>
        </svg>
        Coming Soon
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.placeholder-view {
  min-height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-primary);
  padding: 48px;
}

.placeholder-content {
  text-align: center;
  max-width: 400px;
}

.placeholder-icon {
  width: 120px;
  height: 120px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 32px;
}

.placeholder-icon svg {
  width: 64px;
  height: 64px;
  color: #f59e0b;
}

.placeholder-title {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 12px;
}

.placeholder-description {
  font-size: 16px;
  color: var(--text-secondary);
  line-height: 1.6;
  margin-bottom: 24px;
}

.coming-soon-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(59, 130, 246, 0.1);
  border: 1px solid rgba(59, 130, 246, 0.3);
  color: var(--accent-primary);
  padding: 10px 20px;
  border-radius: 100px;
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 32px;
}

.coming-soon-badge svg {
  width: 18px;
  height: 18px;
}

.back-btn {
  text-transform: none;
}
</style>
