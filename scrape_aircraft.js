const https = require('https');
const fs = require('fs');

// All 398 ICAO codes from Eurocontrol database
const ICAO_CODES = [
  'A10','A109','A124','A129','A19N','A20N','A21N','A225','A306','A30B','A310','A318','A319','A320','A321','A332','A333','A338','A339','A342','A343','A345','A346','A359','A35K','A388','A3ST','A4','A400','A6','A748','AC11','AC50','AC56','AC68','AC6L','AC95','AEST','AJET','AMXM','AN12','AN2','AN22','AN24','AN26','AN28','AN30','AN32','AN38','AN70','AN72','AS32','ASTR','AT43','AT44','AT45','AT72','AT75','ATLA','ATP','B06','B1','B190','B2','B230','B350','B37M','B38M','B39M','B3XM','B412','B430','B461','B462','B463','B52','B701','B703','B712','B720','B722','B731','B732','B733','B734','B735','B736','B737','B738','B739','B741','B742','B743','B744','B748','B74D','B74R','B74S','B752','B753','B762','B763','B764','B772','B773','B77L','B77W','B788','B789','B78X','BA11','BCS1','BCS3','BDOG','BE10','BE20','BE23','BE33','BE36','BE40','BE50','BE55','BE58','BE60','BE70','BE76','BE80','BE95','BE99','BE9L','BER4','BN2T','C06T','C130','C135','C141','C150','C152','C160','C17','C172','C177','C182','C2','C206','C207','C208','C210','C212','C25A','C25B','C25C','C25M','C303','C30J','C310','C337','C340','C402','C404','C414','C421','C425','C441','C5','C525','C526','C550','C560','C56X','C650','C680','C750','C82R','CL2T','CL30','CL35','CL60','CN35','CONC','CP23','CRJ1','CRJ2','CRJ7','CRJ9','CRJX','D228','D28D','D328','DA40','DA42','DA50','DC10','DC85','DC86','DC87','DC91','DC92','DC93','DC94','DC95','DH8A','DH8C','DH8D','DHC6','DHC7','DR40','E110','E120','E121','E135','E145','E170','E190','E195','E2','E3CF','E3TF','E50P','E55P','EC35','EH10','ETAR','EUFI','F1','F100','F104','F117','F14','F15','F16','F18','F2','F26T','F27','F28','F2TH','F35','F4','F406','F5','F50','F70','F900','FA10','FA20','FA50','FA7X','G115','G150','G222','G280','G3','GALX','GL5T','GLEX','GLF3','GLF4','GLF5','H25A','H25B','H25C','H47','H60','H64','H66','HAR','HAWK','HDJT','IL18','IL62','IL76','IL86','IL96','J328','JAGR','JS1','JS20','JS3','JS31','JS32','JS41','K35E','K35R','KA50','KA52','L101','L159','L188','L29B','L39','L410','L610','LJ25','LJ31','LJ35','LJ40','LJ45','LJ55','LJ60','M18','M20P','M20T','M339','M600','MD11','MD81','MD82','MD83','MD87','MD88','MD90','MG21','MG23','MG25','MG29','MG31','MI2','MI8','MIR2','MRF1','MU2','N262','NH90','NIM','P180','P28A','P28R','P28T','P3','P32R','P46T','P808','PA18','PA23','PA27','PA31','PA32','PA34','PA38','PA44','PA46','PAY2','PAY3','PAY4','PC12','PC6T','PC7','PC9','PRM1','PZ04','R135','R44','RALL','RJ1H','RJ70','RJ85','S601','S61','S76','SB05','SB20','SB32','SB37','SB39','SBR2','SC7','SF34','SH33','SH36','SR20','SR22','SU17','SU24','SU25','SU27','SU95','SW2','SW3','SW4','T134','T154','T204','T22M','T37','T38','TAMP','TB30','TBM7','TBM8','TOBA','TOR','TRIN','TUCA','UH1','VC10','YK40','YK42'
];

const CSV_HEADERS = 'icao,name,manufacturer,type,apc,wtc,recat_eu,mtow_kg,v2_ias_kts,takeoff_distance_m,initial_climb_ias_kts,initial_climb_roc_ftmin,climb_fl150_ias_kts,climb_fl150_roc_ftmin,climb_fl240_ias_kts,climb_fl240_roc_ftmin,mach_climb_mach,mach_climb_roc_ftmin,cruise_tas_kt,cruise_mach,cruise_ceiling_fl,range_nm,init_descent_mach,init_descent_rod_ftmin,descent_fl100_ias_kt,descent_fl100_rod_ftmin,approach_ias_kt,approach_rod_ftmin,approach_mcs_kt,vat_ias_kt,landing_distance_m,wing_span_m,length_m,height_m,power_plant,wing_position,engine_position,tail_configuration,landing_gear,iata';

const OUTPUT_FILE = 'aircraft.csv';

function fetchPage(icao) {
  return new Promise((resolve, reject) => {
    const url = `https://learningzone.eurocontrol.int/ilp/customs/ATCPFDB/details.aspx?ICAO=${icao}`;

    https.get(url, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => resolve(data));
      res.on('error', reject);
    }).on('error', reject);
  });
}

function extractData(html, icao) {
  const text = html.replace(/<[^>]+>/g, '\n').replace(/&nbsp;/g, ' ').replace(/&amp;/g, '&');

  const extract = (pattern, group = 1) => {
    const match = text.match(pattern);
    return match ? match[group].trim() : '';
  };

  // Name extraction - look for pattern after ICAO heading
  let name = '';
  const nameMatch = html.match(/<h2[^>]*>[^<]*<\/h2>\s*([^<\n]+)/i);
  if (nameMatch) name = nameMatch[1].trim();

  const data = {
    icao,
    name,
    manufacturer: extract(/\nby\s*\n?\s*([A-Z][A-Za-z\s&-]+?)(?:\n|$)/),
    type: extract(/Type\s*\n?\s*([A-Z][0-9A-Z]+)/),
    apc: extract(/APC\s*\n?\s*([A-E])/),
    wtc: extract(/WTC\s*\n?\s*([LJMH])/),
    recat_eu: extract(/RECAT-EU\s*\n?\s*(Upper Heavy|Lower Heavy|Upper Medium|Lower Medium|Light|Super Heavy)/i),
    mtow_kg: extract(/MTOW[:\s]*\n?\s*(\d+)\s*kg/),
    v2_ias_kts: extract(/V2\s*\(IAS\)[:\s]*\n?\s*(\d+)\s*kts?/),
    takeoff_distance_m: extract(/TAKE-OFF[\s\S]*?Distance[:\s]*\n?\s*(\d+)\s*m/),
    initial_climb_ias_kts: extract(/INITIAL CLIMB[\s\S]*?IAS[:\s]*\n?\s*(\d+)\s*kts/),
    initial_climb_roc_ftmin: extract(/INITIAL CLIMB[\s\S]*?ROC[:\s]*\n?\s*(\d+)\s*ft\/min/),
    climb_fl150_ias_kts: extract(/CLIMB\s*\(to FL 150\)[\s\S]*?IAS[:\s]*\n?\s*(\d+)\s*kts/),
    climb_fl150_roc_ftmin: extract(/CLIMB\s*\(to FL 150\)[\s\S]*?ROC[:\s]*\n?\s*(\d+)\s*ft\/min/),
    climb_fl240_ias_kts: extract(/CLIMB\s*\(to FL 240\)[\s\S]*?IAS[:\s]*\n?\s*(\d+)\s*kts/),
    climb_fl240_roc_ftmin: extract(/CLIMB\s*\(to FL 240\)[\s\S]*?ROC[:\s]*\n?\s*(\d+)\s*ft\/min/),
    mach_climb_mach: extract(/MACH CLIMB[\s\S]*?MACH[:\s]*\n?\s*(0\.\d+)/),
    mach_climb_roc_ftmin: extract(/MACH CLIMB[\s\S]*?ROC[:\s]*\n?\s*(\d+)\s*ft\/min/),
    cruise_tas_kt: extract(/CRUISE[\s\S]*?TAS[:\s]*\n?\s*(\d+)\s*kt/),
    cruise_mach: extract(/CRUISE[\s\S]*?MACH[:\s]*\n?\s*(0\.\d+)/),
    cruise_ceiling_fl: extract(/Ceiling[:\s]*\n?\s*(\d+)\s*FL/),
    range_nm: extract(/Range[:\s]*\n?\s*(\d+)\s*NM/),
    init_descent_mach: extract(/INITIAL DESCENT[\s\S]*?MACH[:\s]*\n?\s*(0\.\d+)/),
    init_descent_rod_ftmin: extract(/INITIAL DESCENT[\s\S]*?ROD[:\s]*\n?\s*(\d+)\s*ft\/min/),
    descent_fl100_ias_kt: extract(/DESCENT\s*\(to FL 100\)[\s\S]*?IAS[:\s]*\n?\s*(\d+)\s*kt/),
    descent_fl100_rod_ftmin: extract(/DESCENT\s*\(to FL 100\)[\s\S]*?ROD[:\s]*\n?\s*(\d+)\s*ft\/min/),
    approach_ias_kt: extract(/APPROACH[\s\S]*?IAS[:\s]*\n?\s*(\d+)\s*kt/),
    approach_rod_ftmin: extract(/APPROACH[\s\S]*?ROD[:\s]*\n?\s*(\d+)\s*ft\/min/),
    approach_mcs_kt: extract(/MCS[:\s]*\n?\s*(\d+)\s*kt/),
    vat_ias_kt: extract(/Vat\s*\(IAS\)[:\s]*\n?\s*(\d+)\s*kt/),
    landing_distance_m: extract(/LANDING[\s\S]*?Distance[:\s]*\n?\s*(\d+)\s*m/),
    wing_span_m: extract(/Wing Span[:\s]*\n?\s*([\d.]+)\s*m/),
    length_m: extract(/Length[:\s]*\n?\s*([\d.]+)\s*m/),
    height_m: extract(/Height[:\s]*\n?\s*([\d.]+)\s*m/),
    power_plant: extract(/Power plant[:\s]*\n?\s*(.+?)(?:\n|Wing position)/s).replace(/\n/g, ' '),
    wing_position: extract(/Wing position[:\s]*\n?\s*(.+?)(?:\n|Engine)/),
    engine_position: extract(/Engine position[:\s]*\n?\s*(.+?)(?:\n|Tail)/),
    tail_configuration: extract(/Tail configuration[:\s]*\n?\s*(.+?)(?:\n|Landing gear)/),
    landing_gear: extract(/Landing gear[:\s]*\n?\s*(.+?)(?:\n|Recognition)/),
    iata: extract(/IATA[:\s]*\n?\s*([A-Z0-9]{2,3}(?:\s*\/\s*[A-Z0-9]{2,3})*)/)
  };

  return data;
}

function escapeCSV(value) {
  if (!value) return '';
  const str = String(value);
  if (str.includes(',') || str.includes('"') || str.includes('\n')) {
    return `"${str.replace(/"/g, '""')}"`;
  }
  return str;
}

function toCSVRow(data) {
  const fields = [
    'icao','name','manufacturer','type','apc','wtc','recat_eu','mtow_kg','v2_ias_kts','takeoff_distance_m',
    'initial_climb_ias_kts','initial_climb_roc_ftmin','climb_fl150_ias_kts','climb_fl150_roc_ftmin',
    'climb_fl240_ias_kts','climb_fl240_roc_ftmin','mach_climb_mach','mach_climb_roc_ftmin',
    'cruise_tas_kt','cruise_mach','cruise_ceiling_fl','range_nm','init_descent_mach','init_descent_rod_ftmin',
    'descent_fl100_ias_kt','descent_fl100_rod_ftmin','approach_ias_kt','approach_rod_ftmin','approach_mcs_kt',
    'vat_ias_kt','landing_distance_m','wing_span_m','length_m','height_m','power_plant',
    'wing_position','engine_position','tail_configuration','landing_gear','iata'
  ];
  return fields.map(f => escapeCSV(data[f])).join(',');
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function main() {
  console.log(`Starting aircraft data extraction for ${ICAO_CODES.length} aircraft...`);
  console.log(`Output file: ${OUTPUT_FILE}`);
  console.log('');

  // Initialize CSV file with headers
  fs.writeFileSync(OUTPUT_FILE, CSV_HEADERS + '\n');

  const allData = [];
  let successCount = 0;
  let errorCount = 0;

  for (let i = 0; i < ICAO_CODES.length; i++) {
    const icao = ICAO_CODES[i];

    try {
      process.stdout.write(`[${i + 1}/${ICAO_CODES.length}] Fetching ${icao}...`);

      const html = await fetchPage(icao);
      const data = extractData(html, icao);
      allData.push(data);

      // Append to CSV
      fs.appendFileSync(OUTPUT_FILE, toCSVRow(data) + '\n');

      successCount++;
      console.log(` OK (${data.manufacturer || 'Unknown'})`);

      // Save progress every 10 aircraft
      if ((i + 1) % 10 === 0) {
        console.log(`\n--- Progress: ${i + 1}/${ICAO_CODES.length} aircraft processed ---\n`);
      }

      // Small delay to be respectful to the server
      await sleep(100);

    } catch (error) {
      errorCount++;
      console.log(` ERROR: ${error.message}`);
    }
  }

  console.log('\n========================================');
  console.log('Extraction complete!');
  console.log(`Success: ${successCount}`);
  console.log(`Errors: ${errorCount}`);
  console.log(`Output saved to: ${OUTPUT_FILE}`);
  console.log('========================================');
}

main().catch(console.error);
