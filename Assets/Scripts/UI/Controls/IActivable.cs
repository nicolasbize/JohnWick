using System;

public interface IActivable
{
    public event EventHandler OnRequestActivation;
    public void Activate();
    public void Deactivate();

}
