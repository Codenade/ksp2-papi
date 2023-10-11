using UnityEngine;

public class ArrowArrangement : MonoBehaviour
{
    public LayerMask mask;
    private MeshRenderer _xMat, _yMat, _zMat;
    private Collider _xCol, _yCol, _zCol;
    private Vector3 _grabPos;
    private bool _isDragging;
    private int _activeAxis;

    private void Start()
    {
        var xArr = gameObject.transform.Find("arrX").gameObject;
        _xMat = xArr.GetComponent<MeshRenderer>();
        _xCol = xArr.GetComponent<Collider>();
        var yArr = gameObject.transform.Find("arrY").gameObject;
        _yMat = yArr.GetComponent<MeshRenderer>();
        _yCol = yArr.GetComponent<Collider>();
        var zArr = gameObject.transform.Find("arrZ").gameObject;
        _zMat = zArr.GetComponent<MeshRenderer>();
        _zCol = zArr.GetComponent<Collider>();
        _xMat.material.color = Color.red;
        _yMat.material.color = Color.green;
        _zMat.material.color = Color.blue;
    }

    private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            if (_isDragging)
            {
                if (_activeAxis == 1)
                {
                    var centerPoint = gameObject.transform.position;
                    var verticalPlane = new Plane(gameObject.transform.forward, centerPoint);
                    var horizontalPlane = new Plane(gameObject.transform.up, centerPoint);
                    float dist;
                    horizontalPlane.Raycast(ray, out var horizontalDistance);
                    verticalPlane.Raycast(ray, out var verticalDistance);
                    dist = Mathf.Min(Mathf.Abs(verticalDistance), Mathf.Abs(horizontalDistance));
                    var rayExtended = dist * ray.direction;
                    var direction = gameObject.transform.InverseTransformPoint(horizontalPlane.ClosestPointOnPlane(Camera.main.transform.position + rayExtended));
                    direction.y = 0;
                    direction.z = 0;
                    direction = gameObject.transform.TransformDirection(direction);
                    var newPos = gameObject.transform.position + direction;
                    newPos -= Vector3.Project(_grabPos, Quaternion.AngleAxis(-90f, gameObject.transform.up) * gameObject.transform.forward);
                    gameObject.transform.position = newPos;
                }
                else if (_activeAxis == 2)
                {
                    var centerPoint = gameObject.transform.position;
                    var xPlane = new Plane(Quaternion.AngleAxis(-90f, gameObject.transform.up) * gameObject.transform.forward, centerPoint);
                    var zPlane = new Plane(gameObject.transform.forward, centerPoint);
                    float dist;
                    xPlane.Raycast(ray, out var xDistance);
                    zPlane.Raycast(ray, out var zDistance);
                    dist = Mathf.Min(Mathf.Abs(zDistance), Mathf.Abs(xDistance));
                    var rayExtended = dist * ray.direction;
                    var direction = gameObject.transform.InverseTransformPoint(xPlane.ClosestPointOnPlane(Camera.main.transform.position + rayExtended));
                    direction.x = 0;
                    direction.z = 0;
                    direction = gameObject.transform.TransformDirection(direction);
                    var newPos = gameObject.transform.position + direction;
                    newPos -= Vector3.Project(_grabPos, gameObject.transform.up);
                    gameObject.transform.position = newPos;
                }
                else if (_activeAxis == 3)
                {
                    var centerPoint = gameObject.transform.position;
                    var verticalPlane = new Plane(Quaternion.AngleAxis(-90f, gameObject.transform.up) * gameObject.transform.forward, centerPoint);
                    var horizontalPlane = new Plane(gameObject.transform.up, centerPoint);
                    float dist;
                    horizontalPlane.Raycast(ray, out var horizontalDistance);
                    verticalPlane.Raycast(ray, out var verticalDistance);
                    dist = Mathf.Min(Mathf.Abs(verticalDistance), Mathf.Abs(horizontalDistance));
                    var rayExtended = dist * ray.direction;
                    var direction = gameObject.transform.InverseTransformPoint(horizontalPlane.ClosestPointOnPlane(Camera.main.transform.position + rayExtended));
                    direction.x = 0;
                    direction.y = 0;
                    direction = gameObject.transform.TransformDirection(direction);
                    var newPos = gameObject.transform.position + direction;
                    newPos -= Vector3.Project(_grabPos, gameObject.transform.forward);
                    gameObject.transform.position = newPos;
                }
            }
            else if (Input.GetMouseButtonDown(0) && _activeAxis != 0)
                _isDragging = true;
        }
        else
        {
            _isDragging = false;
        }
        if (Physics.Raycast(ray, out var hitInfo, 100, mask, QueryTriggerInteraction.Collide))
        {
            if (Input.GetMouseButtonDown(0))
                _grabPos = hitInfo.point - gameObject.transform.position;
            if (hitInfo.collider == _xCol && (_activeAxis == 0 || !_isDragging) || _activeAxis == 1)
            {
                _xMat.material.color = new Color(1, 0.5f, 0.5f);
                _activeAxis = 1;
            }
            else
            {
                _xMat.material.color = Color.red;
                if (hitInfo.collider == _yCol && (_activeAxis == 0 || !_isDragging) || _activeAxis == 2)
                {
                    _yMat.material.color = new Color(0.5f, 1, 0.5f);
                    _activeAxis = 2;
                }
                else
                {
                    _yMat.material.color = Color.green;
                    if (hitInfo.collider == _zCol && (_activeAxis == 0 || !_isDragging) || _activeAxis == 3)
                    {
                        _zMat.material.color = new Color(0.5f, 0.5f, 1);
                        _activeAxis = 3;
                    }
                    else _zMat.material.color = Color.blue;
                }
            }
        }
        else
        {
            if (!_isDragging)
                _activeAxis = 0;
            if (_activeAxis == 1)
                _xMat.material.color = new Color(1, 0.5f, 0.5f);
            else
                _xMat.material.color = Color.red;
            if (_activeAxis == 2)
                _yMat.material.color = new Color(0.5f, 1, 0.5f);
            else
                _yMat.material.color = Color.green;
            if (_activeAxis == 3)
                _zMat.material.color = new Color(0.5f, 0.5f, 1);
            else
                _zMat.material.color = Color.blue;
        }
    }
}