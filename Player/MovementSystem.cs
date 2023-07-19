using Godot;

public class MovementSystem
{
    public Vector3 MoveVector => _moveVector;
    public Vector2 AnimationVector => _animationDirection;

    private float _speed = 20f;
    private float _boostTimer = 0f;
    private float _boostDuration = .2f;
    private Vector2 _boostDirection;
    private Vector2 _previousInputDirection;
    private Vector3 _moveVector;
    private Vector2 _animationDirection;
    private float _boostSpeedScale = 2f;
    private float _boostDampner = 4f;
    private Vector3 _hitDirection;
    private float _hitDampner = 10f;

    public void ProcessHit(float delta)
    {
        _moveVector = _hitDirection;
        _animationDirection = Vector2.Zero;
        _previousInputDirection = Vector2.Zero;
    }

    public void ProcessAttack(Spatial player, Spatial target)
    {
        _boostTimer = 0;

        if (target == null)
        {
            _moveVector = Vector3.Back * _speed * _boostSpeedScale;
            _animationDirection = _boostDirection;
            _animationDirection = Vector2.Down;
            _previousInputDirection = Vector2.Zero;
        }
        else if (target != null && target is Spatial && Object.IsInstanceValid(target))
        {
            var enemy = target as Enemy;
            var distance = player.GlobalTransform.origin.DistanceSquaredTo(target.GlobalTransform.origin);
            if (distance > 3)
            {
                _moveVector = Vector3.Back * _speed * _boostSpeedScale;
                if (enemy != null && enemy.IsDead)
                    _moveVector = Vector3.Back * _speed * .7f;
                _animationDirection = _boostDirection;
                _animationDirection = Vector2.Down;
                _previousInputDirection = Vector2.Zero;
            }
            else
            {
                _moveVector = Vector3.Zero;
                _animationDirection = Vector2.Zero;
                _previousInputDirection = Vector2.Zero;
            }
        }
    }

    public void ProcessInput(Vector2 inputDirection, float delta)
    {
        if (IsBoosting())
        {
            _boostTimer -= delta;
            inputDirection = _previousInputDirection;
        }

        if (!IsBoosting())
        {
            if (_moveVector.Length() > .1f && _moveVector != Vector3.Zero && inputDirection == Vector2.Zero)
                _moveVector = _moveVector.LinearInterpolate(Vector3.Zero, delta * _boostDampner);
            else if (_moveVector.Length() < .1)
                _moveVector = Vector3.Zero;
            else
            {
                var desiredMoveVector = new Vector3(inputDirection.x, 0, inputDirection.y) * _speed;
                _moveVector = _moveVector.LinearInterpolate(desiredMoveVector, delta * 10);
                //_moveVector = desiredMoveVector;
            }

            _animationDirection = _animationDirection.LinearInterpolate(inputDirection, delta * 8);

            if (inputDirection.x != 0 && _previousInputDirection.x != inputDirection.x)
            {
                _boostTimer = _boostDuration;
                _boostDirection = inputDirection.x > 0 ? Vector2.Right : Vector2.Left;
                _moveVector += new Vector3(_boostDirection.x, 0, _boostDirection.y) * _speed * _boostSpeedScale;
                var maxBoostSpeed = _speed * _boostSpeedScale;
                _moveVector = new Vector3(Mathf.Clamp(_moveVector.x, -maxBoostSpeed, maxBoostSpeed), 0, Mathf.Clamp(_moveVector.z, -maxBoostSpeed, maxBoostSpeed));
                _animationDirection = _boostDirection;
            }

            if (inputDirection.y != 0 && _previousInputDirection.y != inputDirection.y)
            {
                _boostTimer = _boostDuration;
                _boostDirection = inputDirection.y > 0 ? Vector2.Down : Vector2.Up;
                _moveVector += new Vector3(_boostDirection.x, 0, _boostDirection.y) * _speed * _boostSpeedScale;
                var maxBoostSpeed = _speed * _boostSpeedScale;
                _moveVector = new Vector3(Mathf.Clamp(_moveVector.x, -maxBoostSpeed, maxBoostSpeed), 0, Mathf.Clamp(_moveVector.z, -maxBoostSpeed, maxBoostSpeed));
                _animationDirection = _boostDirection;
            }

            if (inputDirection != Vector2.Zero)
                _moveVector = ClampMagnitude(_moveVector, _speed);
        }

        _previousInputDirection = inputDirection;
    }

    public bool BoostHasStarted()
    {
        return _boostTimer == _boostDuration;
    }

    public bool BoostHasEnded()
    {
        if (_boostTimer <= 0 && _boostTimer != -1)
        {
            _boostTimer = -1;
            return true;
        }

        return false;
    }

    public bool IsBoosting()
    {
        return _boostTimer > 0;
    }

    public Vector3 ClampMagnitude(Vector3 vector, float min)
    {
        float magnitude = vector.Length();

        if (magnitude < min)
        {
            return vector.Normalized() * min;
        }
        else
        {
            return vector;
        }
    }

    public void GetHit(Vector3 hitDirection)
    {
        _hitDirection = hitDirection;
    }
}