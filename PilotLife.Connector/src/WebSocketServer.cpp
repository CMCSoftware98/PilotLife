#include "WebSocketServer.h"
#include <iostream>

WebSocketServer::WebSocketServer(int port)
    : m_port(port)
    , m_server(port, "127.0.0.1")
{
    m_server.setOnClientMessageCallback(
        [this](std::shared_ptr<ix::ConnectionState> connectionState,
               ix::WebSocket& webSocket,
               const ix::WebSocketMessagePtr& msg) {

            if (msg->type == ix::WebSocketMessageType::Open) {
                std::cout << "Client connected from: "
                          << connectionState->getRemoteIp() << std::endl;

                // Notify callback of new client connection
                if (this->m_clientConnectedCallback) {
                    this->m_clientConnectedCallback(webSocket);
                }
            }
            else if (msg->type == ix::WebSocketMessageType::Close) {
                std::cout << "Client disconnected" << std::endl;
            }
            else if (msg->type == ix::WebSocketMessageType::Error) {
                std::cerr << "WebSocket error: " << msg->errorInfo.reason << std::endl;
            }
            else if (msg->type == ix::WebSocketMessageType::Message) {
                std::cout << "Received message: " << msg->str << std::endl;
                // Handle incoming messages via message handler
                if (this->m_messageHandler) {
                    std::string response = this->m_messageHandler(msg->str, webSocket);
                    if (!response.empty()) {
                        std::cout << "Sending response (" << response.length() << " bytes)" << std::endl;
                        webSocket.send(response);
                    } else {
                        std::cout << "No response to send (empty)" << std::endl;
                    }
                } else {
                    std::cout << "No message handler set" << std::endl;
                }
            }
        }
    );
}

WebSocketServer::~WebSocketServer() {
    stop();
}

bool WebSocketServer::start() {
    auto res = m_server.listen();
    if (!res.first) {
        std::cerr << "Failed to start WebSocket server on port " << m_port
                  << ": " << res.second << std::endl;
        return false;
    }

    m_server.start();
    m_running = true;
    std::cout << "WebSocket server started on ws://127.0.0.1:" << m_port << std::endl;
    return true;
}

void WebSocketServer::stop() {
    if (m_running) {
        m_server.stop();
        m_running = false;
        std::cout << "WebSocket server stopped" << std::endl;
    }
}

void WebSocketServer::broadcast(const std::string& message) {
    std::lock_guard<std::mutex> lock(m_mutex);

    auto clients = m_server.getClients();
    for (auto&& client : clients) {
        if (client->getReadyState() == ix::ReadyState::Open) {
            client->send(message);
        }
    }
}

size_t WebSocketServer::getClientCount() const {
    std::lock_guard<std::mutex> lock(m_mutex);
    // getClients() is non-const, so cast away const (safe for read-only access)
    return const_cast<ix::WebSocketServer&>(m_server).getClients().size();
}
