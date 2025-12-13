// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
use std::net::TcpListener;
use std::process::{Child, Command};
use std::sync::Mutex;
use tauri::{Manager, State};

// State to hold the connector process and port
struct ConnectorState {
    process: Mutex<Option<Child>>,
    port: Mutex<Option<u16>>,
}

/// Find an available TCP port by binding to port 0
fn find_available_port() -> Option<u16> {
    TcpListener::bind("127.0.0.1:0")
        .ok()
        .and_then(|listener| listener.local_addr().ok())
        .map(|addr| addr.port())
}

#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hello, {}! You've been greeted from Rust!", name)
}

/// Start the PilotLife.Connector process and return the port it's listening on
#[tauri::command]
fn start_connector(state: State<'_, ConnectorState>, app: tauri::AppHandle) -> Result<u16, String> {
    let mut process_guard = state.process.lock().map_err(|e| e.to_string())?;
    let mut port_guard = state.port.lock().map_err(|e| e.to_string())?;

    // If connector is already running, return existing port
    if process_guard.is_some() {
        return port_guard.ok_or_else(|| "Connector running but no port".to_string());
    }

    // Find an available port
    let port = find_available_port().ok_or("Failed to find available port")?;

    // Get the path to the connector executable
    let resource_path = app
        .path()
        .resource_dir()
        .map_err(|e| format!("Failed to get resource dir: {}", e))?;

    let connector_path = resource_path.join("PilotLife.Connector.exe");

    // Check if the connector exists
    if !connector_path.exists() {
        return Err(format!(
            "Connector not found at: {}",
            connector_path.display()
        ));
    }

    // Spawn the connector process with the port argument
    let child = Command::new(&connector_path)
        .args(["--port", &port.to_string()])
        .spawn()
        .map_err(|e| format!("Failed to start connector: {}", e))?;

    *process_guard = Some(child);
    *port_guard = Some(port);

    Ok(port)
}

/// Get the port the connector is listening on (if running)
#[tauri::command]
fn get_connector_port(state: State<'_, ConnectorState>) -> Option<u16> {
    state.port.lock().ok().and_then(|guard| *guard)
}

/// Stop the connector process
#[tauri::command]
fn stop_connector(state: State<'_, ConnectorState>) -> Result<(), String> {
    let mut process_guard = state.process.lock().map_err(|e| e.to_string())?;
    let mut port_guard = state.port.lock().map_err(|e| e.to_string())?;

    if let Some(ref mut child) = *process_guard {
        child.kill().map_err(|e| format!("Failed to kill connector: {}", e))?;
        let _ = child.wait(); // Reap the zombie process
    }

    *process_guard = None;
    *port_guard = None;

    Ok(())
}

/// Check if the connector is running
#[tauri::command]
fn is_connector_running(state: State<'_, ConnectorState>) -> bool {
    state
        .process
        .lock()
        .map(|guard| guard.is_some())
        .unwrap_or(false)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_fs::init())
        .manage(ConnectorState {
            process: Mutex::new(None),
            port: Mutex::new(None),
        })
        .invoke_handler(tauri::generate_handler![
            greet,
            start_connector,
            get_connector_port,
            stop_connector,
            is_connector_running
        ])
        .on_window_event(|window, event| {
            // Clean up connector when app closes
            if let tauri::WindowEvent::CloseRequested { .. } = event {
                if let Some(state) = window.try_state::<ConnectorState>() {
                    if let Ok(mut process_guard) = state.process.lock() {
                        if let Some(ref mut child) = *process_guard {
                            let _ = child.kill();
                            let _ = child.wait();
                        }
                        *process_guard = None;
                    }
                    if let Ok(mut port_guard) = state.port.lock() {
                        *port_guard = None;
                    }
                }
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
