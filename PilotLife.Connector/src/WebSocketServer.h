#pragma once

// Must include winsock2.h before any other Windows headers
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

#include <IXWebSocketServer.h>
#include <string>
#include <mutex>
#include <atomic>
#include <functional>
#include <iostream>

class WebSocketServer {
public:
    using ClientConnectedCallback = std::function<void(ix::WebSocket&)>;
    // Message handler receives the message string and the client WebSocket, returns response to send
    using MessageHandler = std::function<std::string(const std::string& message, ix::WebSocket& client)>;

    explicit WebSocketServer(int port);
    ~WebSocketServer();

    // Start/stop server
    bool start();
    void stop();

    // Check if server is running
    bool isRunning() const { return m_running; }

    // Broadcast message to all connected clients
    void broadcast(const std::string& message);

    // Get connected client count
    size_t getClientCount() const;

    // Get the port the server is running on
    int getPort() const { return m_port; }

    // Set callback for when a client connects
    void setClientConnectedCallback(ClientConnectedCallback callback) {
        m_clientConnectedCallback = callback;
    }

    // Set callback for handling incoming messages
    void setMessageHandler(MessageHandler handler) {
        m_messageHandler = handler;
    }

private:
    int m_port;
    ix::WebSocketServer m_server;
    std::atomic<bool> m_running{false};
    mutable std::mutex m_mutex;
    ClientConnectedCallback m_clientConnectedCallback;
    MessageHandler m_messageHandler;
};
