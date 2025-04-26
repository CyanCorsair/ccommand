/**
* Translated to C# from https://gist.github.com/artokun/81260d266945786da05b436b17d2a5f3
* Some aspects had to be adjusted, so it's not 1:1
*/

using Godot;

public partial class RtsCameraController : Node3D
{
    [ExportCategory("Camera speed")]
    [Export(PropertyHint.Range, "0,1000")]
    public float movementSpeed = 15f;

    [Export(PropertyHint.Range, "0,1000, 0.1")]
    public float zoomSpeed = 50f;

    [Export(PropertyHint.Range, "0,1000, 0.1")]
    public float zoomSpeedDampening = 0.5f;

    [Export(PropertyHint.Range, "0,1000, 0.1")]
    public float rotationSpeed = 5f;

    [Export(PropertyHint.Range, "0,10, 0.5")]
    public float edgeSpeed = 0.1f;

    [Export(PropertyHint.Range, "0,10, 0.01")]
    public float panSpeed = 2f;

    [ExportCategory("Camera bounds")]
    [Export(PropertyHint.Range, "0,1000")]
    public int cameraBoundsMargin = 100;

    [Export(PropertyHint.Range, "0,90")]
    public int minElevationAngle = 10;

    [Export(PropertyHint.Range, "0,90")]
    public int maxElevationAngle = 90;

    [Export(PropertyHint.Range, "0,1000")]
    public int minZoom = 5;

    [Export(PropertyHint.Range, "0,1000")]
    public int maxZoom = 50;

    [Export(PropertyHint.Range, "0,1000")]
    public float horizontalEdgeMargin = 100f;

    [Export(PropertyHint.Range, "0,1000")]
    public float verticalEdgeMargin = 50f;

    [ExportCategory("Additional settings")]
    [Export]
    bool allowRotation = true;

    [Export]
    bool invertedY = false;

    [Export]
    bool zoomToCursor = true;

    [Export]
    bool allowPan = true;

    // Movement
    Vector2 lastMousePosition = new Vector2();
    bool isRotating = false;
    Node3D elevationInstance;

    // Zoom
    int zoomDirection = 0;
    Camera3D cameraInstance;
    Plane GROUND_PLANE = new Plane(Vector3.Up, 0);
    const int RAY_LENGTH = 1000;

    // Pan
    bool isPanning = false;
    bool isEdgePanningX = false;
    bool isEdgePanningY = false;
    bool isMoving = false;
    Vector3 lastDragPoint;

    bool isMouseInWindow = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        elevationInstance = GetNode<Node3D>("Elevation");
        cameraInstance = GetNode<Camera3D>("Elevation/MainCamera");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (isMouseInWindow)
        {
            EdgeMove(delta);
            MoveSelf(delta);
            //RotateSelf(delta);
            ZoomSelf(null);
            //PanSelf();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("camera_rotate"))
        {
            isRotating = true;
            lastMousePosition = GetViewport().GetMousePosition();
        }
        if (@event.IsActionReleased("camera_rotate"))
        {
            isRotating = false;
        }

        if (@event.IsActionPressed("camera_zoom_in"))
        {
            zoomDirection = -1;
        }
        if (@event.IsActionPressed("camera_zoom_out"))
        {
            zoomDirection = 1;
        }

        if (@event.IsActionPressed("camera_pan"))
        {
            isPanning = true;
            lastMousePosition = GetViewport().GetMousePosition();
            lastDragPoint = GetGroundClickLocation();
        }
        if (@event.IsActionReleased("camera_pan"))
        {
            isPanning = false;
            lastDragPoint = Position;
        }

        if (@event.IsActionPressed("main_click"))
        {
            Vector3 clickLocation = GetGroundClickLocation();
            HexCoordinates coords = HexCoordinates.FromPosition(clickLocation);
            GD.Print("Click coordinates: " + coords.ToString());
            GD.Print("Click location: " + clickLocation);
        }

        if (@event.IsActionPressed("secondary_click"))
        {
            Vector3 clickLocation = GetGroundClickLocation();
            GD.Print("Click location 2: " + clickLocation);
        }
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationWMMouseExit)
        {
            isMouseInWindow = false;
        }

        if (notification == NotificationWMMouseEnter)
        {
            isMouseInWindow = true;
        }
    }

    private void MoveSelf(double delta)
    {
        Vector3 velocity = new Vector3();

        if (Input.IsActionPressed("camera_forward"))
        {
            isMoving = true;
            velocity -= Transform.Basis.Z;
        }
        if (Input.IsActionPressed("camera_backward"))
        {
            isMoving = true;
            velocity += Transform.Basis.Z;
        }
        if (Input.IsActionPressed("camera_left"))
        {
            isMoving = true;
            velocity -= Transform.Basis.X;
        }
        if (Input.IsActionPressed("camera_right"))
        {
            isMoving = true;
            velocity += Transform.Basis.X;
        }

        if (
            !Input.IsActionPressed("camera_forward")
            && !Input.IsActionPressed("camera_backward")
            && !Input.IsActionPressed("camera_left")
            && !Input.IsActionPressed("camera_right")
        )
        {
            isMoving = false;
        }

        if (isMoving)
        {
            velocity = velocity.Normalized();
            GlobalTranslate(velocity * (float)delta * movementSpeed);
        }
    }

    private void RotateSelf(double delta)
    {
        if (!isRotating || !allowRotation)
        {
            return;
        }

        Vector2 mouseDisplacement = GetMouseDisplacement();
        RotateLeftRight(delta, mouseDisplacement.X);
        ElevateSelf(delta, mouseDisplacement.Y);
    }

    private void EdgeMove(double delta)
    {
        Vector3 velocity = new Vector3();
        Viewport viewport = GetViewport();
        Rect2 visibleRect = viewport.GetVisibleRect();
        Vector2 mousePosition = viewport.GetMousePosition();

        if (mousePosition.X < verticalEdgeMargin)
        {
            isEdgePanningX = true;
            velocity.X -= Mathf.Clamp(Mathf.Abs(mousePosition.X - verticalEdgeMargin), -5, 5);
        }
        else if (mousePosition.X > visibleRect.Size.X - verticalEdgeMargin)
        {
            isEdgePanningX = true;
            velocity.X += Mathf.Clamp(Mathf.Abs(mousePosition.X - visibleRect.Size.X), -5, 5);
        }
        else
        {
            isEdgePanningX = false;
        }

        if (mousePosition.Y < horizontalEdgeMargin)
        {
            isEdgePanningY = true;
            velocity.Z -= Mathf.Clamp(Mathf.Abs(mousePosition.Y - horizontalEdgeMargin), -5, 5);
        }
        else if (mousePosition.Y > visibleRect.Size.Y - horizontalEdgeMargin)
        {
            isEdgePanningY = true;
            velocity.Z += Mathf.Clamp(
                Mathf.Abs(mousePosition.Y - visibleRect.Size.Y + horizontalEdgeMargin),
                -5,
                5
            );
        }
        else
        {
            isEdgePanningY = false;
        }

        if (isEdgePanningX && isEdgePanningY)
        {
            velocity.Y /=
                cameraInstance.Position.Z != 0 ? Mathf.Clamp(cameraInstance.Position.Z, -5, 5) : 5f;
            velocity.X /=
                cameraInstance.Position.Z != 0
                    ? Mathf.Clamp(cameraInstance.Position.Z, -15, 15)
                    : 2.5f;
            GlobalTranslate(velocity * (float)delta * movementSpeed);
        }
        else
        {
            if (isEdgePanningX)
            {
                GlobalTranslate(velocity * (float)delta * movementSpeed);
            }

            if (isEdgePanningY)
            {
                GlobalTranslate(velocity * (float)delta * movementSpeed / 2);
            }
        }
    }

    private void PanSelf()
    {
        if (!isPanning || !allowPan)
        {
            return;
        }

        Vector3 newPosition = Position + lastDragPoint + GetGroundClickLocation();
        Position = Position.Lerp(newPosition, 0.5f);
    }

    private void ZoomSelf(int? forcedZoomDirection)
    {
        if (zoomDirection == 0 && forcedZoomDirection == null)
        {
            return;
        }
        else if (zoomDirection == 0 && forcedZoomDirection != null)
        {
            zoomDirection = (int)forcedZoomDirection;
        }

        var cameraHeight = Position.Y != 0 ? Position.Y : 10f;
        double newZoom = Mathf.Clamp((cameraHeight + zoomSpeed) * zoomDirection, minZoom, maxZoom);

        if (zoomDirection < 0)
        {
            GD.Print("ZOOM < 0");
            GD.Print("X: " + Position.X);
            GD.Print("Y: " + newZoom);
            GD.Print("Z: " + Position.Z);
            GlobalTranslate(new Vector3(Position.X, (float)newZoom, Position.Z) * -1);
        }
        else if (zoomDirection > 0)
        {
            GD.Print("ZOOM > 0");
            GD.Print("X: " + Position.X);
            GD.Print("Y: " + newZoom);
            GD.Print("Z: " + Position.Z);
            GlobalTranslate(new Vector3(Position.X, (float)newZoom, Position.Z));
        }

        zoomDirection = 0;
    }

    private Vector2 GetMouseDisplacement()
    {
        Vector2 currentMousePosition = GetViewport().GetMousePosition();
        Vector2 displacement = currentMousePosition - lastMousePosition;
        lastMousePosition = currentMousePosition;
        return displacement;
    }

    private void RotateLeftRight(double delta, float x)
    {
        Rotation.Rotated(
            new Vector3(0, 1, 0),
            (float)(Rotation.Y + (Mathf.DegToRad(x * delta * rotationSpeed) * -1))
        );
    }

    private void ElevateSelf(double delta, float y)
    {
        float newElevation = Mathf.RadToDeg(elevationInstance.Rotation.X);

        if (invertedY)
        {
            newElevation += (float)(y * delta * rotationSpeed);
        }
        else
        {
            newElevation -= (float)(y * delta * rotationSpeed);
        }

        newElevation = Mathf.Clamp(newElevation, -maxElevationAngle, -minElevationAngle);
        elevationInstance.Rotate(new Vector3(1, 0, 0), Mathf.DegToRad(newElevation));
    }

    private Vector3 GetGroundClickLocation()
    {
        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector3 rayFrom = cameraInstance.ProjectRayOrigin(mousePosition);
        Vector3 rayTo = rayFrom + cameraInstance.ProjectRayNormal(mousePosition) * RAY_LENGTH;

        if (GROUND_PLANE.IntersectsRay(rayFrom, rayTo) != null)
        {
            return (Vector3)GROUND_PLANE.IntersectsRay(rayFrom, rayTo);
        }

        return Vector3.Zero;
    }

    private void RealignCamera(Vector3 location)
    {
        Vector3 clickLocation = GetGroundClickLocation();
        Vector3 displacement = location - clickLocation;
        Translate(displacement);
    }
}
