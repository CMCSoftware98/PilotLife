const sharp = require('sharp');
const fs = require('fs');
const path = require('path');

const iconsDir = path.join(__dirname, '..', 'src-tauri', 'icons');

// Icon sizes needed for Tauri
const sizes = [
  { name: '32x32.png', size: 32 },
  { name: '128x128.png', size: 128 },
  { name: '128x128@2x.png', size: 256 },
  { name: 'icon.png', size: 512 },
  // Windows Store logos
  { name: 'Square30x30Logo.png', size: 30 },
  { name: 'Square44x44Logo.png', size: 44 },
  { name: 'Square71x71Logo.png', size: 71 },
  { name: 'Square89x89Logo.png', size: 89 },
  { name: 'Square107x107Logo.png', size: 107 },
  { name: 'Square142x142Logo.png', size: 142 },
  { name: 'Square150x150Logo.png', size: 150 },
  { name: 'Square284x284Logo.png', size: 284 },
  { name: 'Square310x310Logo.png', size: 310 },
  { name: 'StoreLogo.png', size: 50 },
];

// Create a plane icon SVG with gradient background
function createIconSvg(size) {
  const padding = Math.floor(size * 0.15);
  const planeSize = size - (padding * 2);

  return `
<svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <linearGradient id="bgGradient" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#0ea5e9"/>
      <stop offset="100%" style="stop-color:#8b5cf6"/>
    </linearGradient>
    <linearGradient id="planeGradient" x1="0%" y1="0%" x2="100%" y2="0%">
      <stop offset="0%" style="stop-color:#ffffff"/>
      <stop offset="100%" style="stop-color:#e0f2fe"/>
    </linearGradient>
  </defs>

  <!-- Rounded rectangle background -->
  <rect width="${size}" height="${size}" rx="${size * 0.2}" ry="${size * 0.2}" fill="url(#bgGradient)"/>

  <!-- Plane icon centered -->
  <g transform="translate(${padding}, ${padding})">
    <svg width="${planeSize}" height="${planeSize}" viewBox="0 0 24 24" fill="url(#planeGradient)">
      <path d="M21 16v-2l-8-5V3.5c0-.83-.67-1.5-1.5-1.5S10 2.67 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L13 19v-5.5l8 2.5z"/>
    </svg>
  </g>
</svg>`;
}

async function generateIcons() {
  console.log('Generating PilotLife app icons...\n');

  // Ensure icons directory exists
  if (!fs.existsSync(iconsDir)) {
    fs.mkdirSync(iconsDir, { recursive: true });
  }

  for (const { name, size } of sizes) {
    const svg = createIconSvg(size);
    const outputPath = path.join(iconsDir, name);

    await sharp(Buffer.from(svg))
      .png()
      .toFile(outputPath);

    console.log(`✓ Generated ${name} (${size}x${size})`);
  }

  // Generate ICO file for Windows (contains multiple sizes)
  console.log('\nGenerating Windows ICO file...');

  const icoSizes = [16, 24, 32, 48, 64, 128, 256];
  const icoBuffers = [];

  for (const size of icoSizes) {
    const svg = createIconSvg(size);
    const pngBuffer = await sharp(Buffer.from(svg))
      .png()
      .toBuffer();
    icoBuffers.push({ size, buffer: pngBuffer });
  }

  // Create ICO file manually
  const icoBuffer = createIcoFile(icoBuffers);
  fs.writeFileSync(path.join(iconsDir, 'icon.ico'), icoBuffer);
  console.log('✓ Generated icon.ico');

  // Generate ICNS for macOS
  console.log('\nGenerating macOS ICNS file...');
  const icnsPath = path.join(iconsDir, 'icon.icns');

  // For ICNS, we'll create a 512x512 and 1024x1024 PNG and use them
  // Note: Full ICNS generation requires additional tooling, so we'll create a placeholder
  const svg1024 = createIconSvg(1024);
  await sharp(Buffer.from(svg1024))
    .png()
    .toFile(path.join(iconsDir, 'icon_512x512@2x.png'));

  console.log('✓ Generated icon_512x512@2x.png (for ICNS conversion)');
  console.log('\nNote: For proper ICNS file, run: iconutil -c icns icon.iconset');

  console.log('\n✅ All icons generated successfully!');
}

// Create ICO file from PNG buffers
function createIcoFile(images) {
  // ICO file format:
  // - ICONDIR header (6 bytes)
  // - ICONDIRENTRY for each image (16 bytes each)
  // - Image data

  const headerSize = 6;
  const entrySize = 16;
  const numImages = images.length;

  // Calculate total size
  let dataOffset = headerSize + (entrySize * numImages);
  const entries = [];

  for (const { size, buffer } of images) {
    entries.push({
      width: size >= 256 ? 0 : size,
      height: size >= 256 ? 0 : size,
      buffer,
      offset: dataOffset
    });
    dataOffset += buffer.length;
  }

  // Create the ICO buffer
  const totalSize = dataOffset;
  const ico = Buffer.alloc(totalSize);

  // Write ICONDIR header
  ico.writeUInt16LE(0, 0);        // Reserved, must be 0
  ico.writeUInt16LE(1, 2);        // Image type: 1 = ICO
  ico.writeUInt16LE(numImages, 4); // Number of images

  // Write ICONDIRENTRY for each image
  let entryOffset = headerSize;
  for (let i = 0; i < entries.length; i++) {
    const entry = entries[i];
    const size = images[i].size;

    ico.writeUInt8(entry.width, entryOffset);      // Width
    ico.writeUInt8(entry.height, entryOffset + 1); // Height
    ico.writeUInt8(0, entryOffset + 2);            // Color palette
    ico.writeUInt8(0, entryOffset + 3);            // Reserved
    ico.writeUInt16LE(1, entryOffset + 4);         // Color planes
    ico.writeUInt16LE(32, entryOffset + 6);        // Bits per pixel
    ico.writeUInt32LE(entry.buffer.length, entryOffset + 8);  // Image size
    ico.writeUInt32LE(entry.offset, entryOffset + 12);        // Image offset

    entryOffset += entrySize;
  }

  // Write image data
  for (const entry of entries) {
    entry.buffer.copy(ico, entry.offset);
  }

  return ico;
}

generateIcons().catch(console.error);
