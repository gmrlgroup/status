import asyncio
import platform
import subprocess
import time
from datetime import datetime
from typing import Optional, Dict, Any
from pydantic import BaseModel, Field, validator
import socket


class PingResponse(BaseModel):
    """
    Model representing the response from a ping operation.
    """
    host: str = Field(..., description="The host that was pinged")
    success: bool = Field(..., description="Whether the ping was successful")
    response_time: Optional[float] = Field(None, description="Response time in milliseconds")
    packet_loss: Optional[float] = Field(None, description="Packet loss percentage")
    error_message: Optional[str] = Field(None, description="Error message if ping failed")
    timestamp: datetime = Field(default_factory=datetime.utcnow, description="When the ping was performed")
    packets_sent: int = Field(default=4, description="Number of packets sent")
    packets_received: int = Field(default=0, description="Number of packets received")

    @validator('packet_loss')
    def validate_packet_loss(cls, v):
        """Ensure packet loss is between 0 and 100"""
        if v is not None and (v < 0 or v > 100):
            raise ValueError('Packet loss must be between 0 and 100')
        return v

    @validator('response_time')
    def validate_response_time(cls, v):
        """Ensure response time is not negative"""
        if v is not None and v < 0:
            raise ValueError('Response time cannot be negative')
        return v


class PingServer(BaseModel):
    """
    Model for ping server operations with methods to ping hosts and return responses.
    """
    
    host: str = Field(..., min_length=1, description="Target host to ping (IP address or hostname)")
    port: Optional[int] = Field(None, ge=1, le=65535, description="Optional port for TCP ping")
    timeout: int = Field(default=5, ge=1, le=60, description="Timeout in seconds")
    packet_count: int = Field(default=4, ge=1, le=100, description="Number of ping packets to send")
    
    @validator('host')
    def validate_host(cls, v):
        """Basic validation for host format"""
        if not v or v.strip() == "":
            raise ValueError('Host cannot be empty')
        return v.strip()

    def ping_icmp(self) -> PingResponse:
        """
        Perform ICMP ping using system ping command.
        
        Returns:
            PingResponse: Result of the ping operation
        """
        try:
            # Determine ping command based on OS
            system = platform.system().lower()
            if system == "windows":
                cmd = ["ping", "-n", str(self.packet_count), "-w", str(self.timeout * 1000), self.host]
            else:  # Linux/macOS
                cmd = ["ping", "-c", str(self.packet_count), "-W", str(self.timeout), self.host]
            
            start_time = time.time()
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=self.timeout + 5)
            end_time = time.time()
            
            if result.returncode == 0:
                return self._parse_ping_output(result.stdout, True, end_time - start_time)
            else:
                return PingResponse(
                    host=self.host,
                    success=False,
                    error_message=result.stderr or "Ping failed",
                    packets_sent=self.packet_count,
                    packets_received=0,
                    packet_loss=100.0
                )
                
        except subprocess.TimeoutExpired:
            return PingResponse(
                host=self.host,
                success=False,
                error_message="Ping timeout",
                packets_sent=self.packet_count,
                packets_received=0,
                packet_loss=100.0
            )
        except Exception as e:
            return PingResponse(
                host=self.host,
                success=False,
                error_message=f"Ping error: {str(e)}",
                packets_sent=self.packet_count,
                packets_received=0,
                packet_loss=100.0
            )

    def ping_tcp(self, port: Optional[int] = None) -> PingResponse:
        """
        Perform TCP ping by attempting to connect to a specific port.
        
        Args:
            port: Port to connect to (uses self.port if not specified)
            
        Returns:
            PingResponse: Result of the TCP ping operation
        """
        target_port = port or self.port
        if not target_port:
            return PingResponse(
                host=self.host,
                success=False,
                error_message="No port specified for TCP ping",
                packets_sent=1,
                packets_received=0,
                packet_loss=100.0
            )
        
        try:
            start_time = time.time()
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(self.timeout)
            
            result = sock.connect_ex((self.host, target_port))
            end_time = time.time()
            
            sock.close()
            
            response_time = (end_time - start_time) * 1000  # Convert to milliseconds
            
            if result == 0:
                return PingResponse(
                    host=f"{self.host}:{target_port}",
                    success=True,
                    response_time=response_time,
                    packets_sent=1,
                    packets_received=1,
                    packet_loss=0.0
                )
            else:
                return PingResponse(
                    host=f"{self.host}:{target_port}",
                    success=False,
                    error_message=f"Connection failed to port {target_port}",
                    response_time=response_time,
                    packets_sent=1,
                    packets_received=0,
                    packet_loss=100.0
                )
                
        except socket.timeout:
            return PingResponse(
                host=f"{self.host}:{target_port}",
                success=False,
                error_message="TCP ping timeout",
                packets_sent=1,
                packets_received=0,
                packet_loss=100.0
            )
        except Exception as e:
            return PingResponse(
                host=f"{self.host}:{target_port}",
                success=False,
                error_message=f"TCP ping error: {str(e)}",
                packets_sent=1,
                packets_received=0,
                packet_loss=100.0
            )

    async def ping_async(self, use_tcp: bool = False, port: Optional[int] = None) -> PingResponse:
        """
        Perform asynchronous ping operation.
        
        Args:
            use_tcp: Whether to use TCP ping instead of ICMP
            port: Port for TCP ping (uses self.port if not specified)
            
        Returns:
            PingResponse: Result of the ping operation
        """
        loop = asyncio.get_event_loop()
        
        if use_tcp:
            return await loop.run_in_executor(None, self.ping_tcp, port)
        else:
            return await loop.run_in_executor(None, self.ping_icmp)

    def ping(self, use_tcp: bool = False, port: Optional[int] = None) -> PingResponse:
        """
        Main ping method that can perform either ICMP or TCP ping.
        
        Args:
            use_tcp: Whether to use TCP ping instead of ICMP ping
            port: Port for TCP ping (uses self.port if not specified)
            
        Returns:
            PingResponse: Result of the ping operation
        """
        if use_tcp:
            return self.ping_tcp(port)
        else:
            return self.ping_icmp()

    def _parse_ping_output(self, output: str, success: bool, total_time: float) -> PingResponse:
        """
        Parse ping command output to extract statistics.
        
        Args:
            output: Raw ping command output
            success: Whether the ping was successful
            total_time: Total time taken for the ping operation
            
        Returns:
            PingResponse: Parsed ping response
        """
        packets_sent = self.packet_count
        packets_received = 0
        avg_response_time = None
        packet_loss = 100.0
        
        try:
            # Parse output based on OS
            system = platform.system().lower()
            lines = output.lower().split('\n')
            
            if system == "windows":
                # Windows ping output parsing
                for line in lines:
                    if "packets: sent" in line:
                        # Example: "Packets: Sent = 4, Received = 4, Lost = 0 (0% loss)"
                        parts = line.split(',')
                        for part in parts:
                            if "received" in part:
                                packets_received = int(part.split('=')[1].strip())
                            elif "% loss" in part:
                                loss_str = part.split('(')[1].split('%')[0]
                                packet_loss = float(loss_str)
                    elif "average" in line and "ms" in line:
                        # Example: "Minimum = 1ms, Maximum = 4ms, Average = 2ms"
                        avg_time_str = line.split("average = ")[1].split("ms")[0]
                        avg_response_time = float(avg_time_str)
            else:
                # Linux/macOS ping output parsing
                for line in lines:
                    if "packets transmitted" in line:
                        # Example: "4 packets transmitted, 4 received, 0% packet loss"
                        parts = line.split(',')
                        packets_received = int(parts[1].split()[0])
                        loss_str = parts[2].split('%')[0].strip()
                        packet_loss = float(loss_str)
                    elif "avg" in line and "/" in line:
                        # Example: "rtt min/avg/max/mdev = 1.234/2.345/3.456/0.123 ms"
                        times = line.split('=')[1].split('/')
                        if len(times) >= 2:
                            avg_response_time = float(times[1])
                            
        except (ValueError, IndexError, AttributeError):
            # If parsing fails, use defaults
            pass
        
        return PingResponse(
            host=self.host,
            success=success and packets_received > 0,
            response_time=avg_response_time,
            packets_sent=packets_sent,
            packets_received=packets_received,
            packet_loss=packet_loss
        )

    def __str__(self) -> str:
        """String representation of the ping server"""
        return f"PingServer(host='{self.host}', timeout={self.timeout}s)"

    def __repr__(self) -> str:
        """Detailed string representation"""
        return (
            f"PingServer("
            f"host='{self.host}', "
            f"port={self.port}, "
            f"timeout={self.timeout}, "
            f"packet_count={self.packet_count})"
        )

    class Config:
        """Pydantic configuration"""
        validate_assignment = True
        schema_extra = {
            "example": {
                "host": "google.com",
                "port": 80,
                "timeout": 5,
                "packet_count": 4
            }
        }
