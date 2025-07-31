from datetime import datetime
from enum import Enum
from typing import Optional
from pydantic import BaseModel, Field, validator


class EntityStatus(Enum):
    """Entity status enumeration matching the C# EntityStatus enum"""
    UNKNOWN = 0
    ONLINE = 1
    OFFLINE = 2
    DEGRADED = 3
    MAINTENANCE = 4
    ERROR = 5


class EntityStatusHistory(BaseModel):
    """
    Pydantic model for EntityStatusHistory matching the C# model structure.
    
    This model represents the status history of an entity with monitoring data
    including response times, uptime percentages, and status messages.
    """
    
    id: Optional[int] = Field(None, description="Auto-generated primary key")
    entity_id: str = Field(..., min_length=1, description="Required entity identifier")
    status: EntityStatus = Field(..., description="Current status of the entity")
    status_message: Optional[str] = Field(
        None, 
        max_length=2000, 
        description="Optional status message with details"
    )
    response_time: Optional[float] = Field(
        None, 
        ge=0, 
        description="Response time in milliseconds"
    )
    uptime_percentage: Optional[float] = Field(
        None, 
        ge=0, 
        le=100, 
        description="Uptime percentage at the time of this status check"
    )
    checked_at: datetime = Field(
        default_factory=lambda: datetime.utcnow(),
        description="Timestamp when the status was checked"
    )
    
    # BaseModel properties (from C# BaseModel)
    workspace_id: Optional[str] = Field(None, description="Workspace identifier")
    created_on: Optional[datetime] = Field(
        default_factory=lambda: datetime.utcnow(),
        description="Record creation timestamp"
    )
    modified_on: Optional[datetime] = Field(
        default_factory=lambda: datetime.utcnow(),
        description="Record modification timestamp"
    )
    created_by: Optional[str] = Field(None, description="User who created the record")
    modified_by: Optional[str] = Field(None, description="User who last modified the record")
    is_deleted: bool = Field(False, description="Soft delete flag")
    deleted_by: Optional[str] = Field(None, description="User who deleted the record")
    deleted_at: Optional[datetime] = Field(None, description="Deletion timestamp")

    @validator('response_time')
    def validate_response_time(cls, v):
        """Ensure response time is not negative"""
        if v is not None and v < 0:
            raise ValueError('Response time cannot be negative')
        return v

    @validator('uptime_percentage')
    def validate_uptime_percentage(cls, v):
        """Ensure uptime percentage is between 0 and 100"""
        if v is not None and (v < 0 or v > 100):
            raise ValueError('Uptime percentage must be between 0 and 100')
        return v

    @validator('status_message')
    def validate_status_message_length(cls, v):
        """Ensure status message doesn't exceed maximum length"""
        if v is not None and len(v) > 2000:
            raise ValueError('Status message cannot exceed 2000 characters')
        return v

    class Config:
        """Pydantic configuration"""
        use_enum_values = True
        validate_assignment = True
        arbitrary_types_allowed = True
        schema_extra = {
            "example": {
                "entity_id": "entity-123",
                "status": 1,
                "status_message": "Service is running normally",
                "response_time": 150.5,
                "uptime_percentage": 99.9,
                "checked_at": "2025-07-24T10:30:00Z",
                "workspace_id": "workspace-456"
            }
        }

    def __str__(self) -> str:
        """String representation of the entity status history"""
        return f"EntityStatusHistory(id={self.id}, entity_id={self.entity_id}, status={self.status.name})"

    def __repr__(self) -> str:
        """Detailed string representation"""
        return (
            f"EntityStatusHistory("
            f"id={self.id}, "
            f"entity_id='{self.entity_id}', "
            f"status={self.status.name}, "
            f"checked_at={self.checked_at})"
        )
