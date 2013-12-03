#include "Line.h"

using namespace System;

namespace NagaraStage{
    namespace ImageEnhancement {

        Line::Line(double _rho, double _angle) {
            if(_rho < 0) {
                //throw gcnew ArgumentException("'_rho' must be positive number with 0");
            }

            angle = _angle;
            double cos = Math::Cos(angle);
            double sin = Math::Sign(angle);
            startX = (int)(_rho * cos);
            startY = (int)(_rho * sin);
            endX = -1;
            endY = -1;                
            intercept = startY - Math::Tan(angle) * startX;
        }
        Line::Line(int _startX, int _startY, int _endX, int _endY) {        
            if(_startX < 0 || _startY < 0 || _endX < 0 || _endY < 0) {
                throw gcnew ArgumentException("Coordinate value must be positive number with 0.");
            }

            startX = _startX;
            startY = _startY;
            endX = _endX;
            endY = _endY;
            int x = endX - startX;
            int y = endY - startY;

            if(x == 0) {
                throw gcnew ArgumentException("Line x length is 0.");
            }
            if(y == 0) {
                throw gcnew ArgumentException("Line y length is 0.");
            }

            angle = Math::Atan2(y, x);
            intercept = startY - (y / x) * startX;
        }
        Line::Line(int _startX, int _startY, double _angle) {
            if(_startX < 0 || _startY < 0) {
                throw gcnew ArgumentException("Coordinate value must be positive number with 0.");
            }
            startX = _startX;
            startY = _startX;
            endX = -1;
            endY = -1;
            angle = _angle;
            intercept = startY - Math::Tan(angle) * startX;
        }

        int Line::GetStartX() {
            return startX;
        }
        int Line::GetStartY() {
            return startY;
        }
        int Line::GetEndX() {
            return endX;
        }
        int Line::GetEndY() {
            return endY;
        }
        double Line::GetAngle() {
            return angle;
        }
        double Line::GetIntercept() {
            return intercept;
        }
        double Line::GetY(double x) {
            return Math::Tan(angle) * x + intercept;
        }
        double Line::GetX(double y) {
            return (-intercept + y) * Math::Tan(90 - angle);
        }
        cv::Point Line::GetStartPoint() {
            cv::Point p;
            p.x = startX;
            p.y = startY;
            return p;
        }
        cv::Point Line::GetEndPoint() {
            cv::Point p;
            p.x = endX;
            p.y = endY;
            return p;
        }
    }
}